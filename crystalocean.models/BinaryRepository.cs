using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql;

namespace CrystalOcean.Data.Models.Repository
{
    public class BinaryRepository : DbContext
    {
        public BinaryRepository(DbContextOptions<BinaryRepository> options) 
            : base(options)
        {
        }

        public DbSet<Image> Images { get; set; }

        public TEntity Insert<TEntity>(TEntity binary, Stream input) where TEntity : Binary
        {
            using (var trans = this.Database.BeginTransaction())
            {
                if (!input.CanRead)
                    throw new Exception("Invalid stream.");
                WriteLargeObject(binary, input);

                this.Entry(binary).State = EntityState.Added;
                this.SaveChanges();
                trans.Commit();
                
                return binary;
            }
        }

        public async Task<TEntity> InsertAsync<TEntity>(TEntity binary, Func<Stream, Task> action) where TEntity : Binary
        {
            using (var trans = this.Database.BeginTransaction())
            {
                var manager = CreateLargeObjectManager();
                uint oid = manager.Create();
                using (Stream output = manager.OpenReadWrite(oid))
                {
                    await action(output);
                }
                binary.ObjectId = oid;
                trans.Commit();
                this.Entry(binary).State = EntityState.Added;
                this.SaveChanges();
                return binary;
            }
        }

        public void Delete<TEntity>(TEntity binary) where TEntity : Binary
        {
            using (var trans = this.Database.BeginTransaction())
            {
                // Retrieve a Large Object Manager for the connection
                CreateLargeObjectManager().Unlink(binary.ObjectId);  
                this.Remove(binary);
                this.SaveChanges();
            }
        }

        public async Task<TEntity> ExportToStreamAsync<TEntity>(TEntity binary, Stream output) where TEntity : Binary
        {
            using (var trans = this.Database.BeginTransaction())
            {
                if (!output.CanWrite)
                    throw new Exception("Invalid stream.");
                await ReadLargeObject(binary, output);
                output.Close();
            }
            return binary;
        }

        public async Task<TEntity> ExportToFileAsync<TEntity>(TEntity binary, String path) where TEntity : Binary
        {
            using (var trans = this.Database.BeginTransaction())
            {
                using (var output = new FileStream(path, FileMode.Create))
                {
                    await ReadLargeObject(binary, output);   
                }
            }
            return binary;
        }

        private void WriteLargeObject<TEntity>(TEntity binary, Stream input) where TEntity : Binary
        {
            var manager = CreateLargeObjectManager();
            uint oid = manager.Create();
            using (var output = manager.OpenReadWrite(oid))
            {
                input.CopyTo(output);
            }
            binary.ObjectId = oid;
        }

        private async Task<TEntity> ReadLargeObject<TEntity>(TEntity binary, Stream output) where TEntity : Binary
        {
            using (var input = CreateLargeObjectManager().OpenRead(binary.ObjectId))
            {
                await input.CopyToAsync(output);
            }
            return binary;
        }

        private NpgsqlLargeObjectManager CreateLargeObjectManager()
        {
            return new NpgsqlLargeObjectManager(this.Database.GetDbConnection() as NpgsqlConnection);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Binary>()
                .HasDiscriminator<String>("Type")
                .HasValue<Image>("Image");

            // shadow properties
            builder.Entity<Image>()
                .Property<Nullable<DateTime>>("UpdateTime");

            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            UpdateProperties<Image>();
            return base.SaveChanges();
        }

        private void UpdateProperties<TEntity>() where TEntity : Binary
        {
            var modifiedSourceInfo =
                ChangeTracker.Entries<TEntity>()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in modifiedSourceInfo)
            {
                entry.Property("UpdateTime").CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
