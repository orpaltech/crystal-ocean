using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrystalOcean.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql;

namespace CrystalOcean.Data.Repository
{
    public class BinaryRepository : DbContext
    {
        public BinaryRepository(DbContextOptions<BinaryRepository> options) 
            : base(options)
        {
        }

        public DbSet<Image> Images { get; set; }

        public async Task<TEntity> InsertAsync<TEntity>(Func<Stream, Task> streamWriter, Func<Task<TEntity>> entitySupplier) where TEntity : Binary
        {
            using (var trans = this.Database.BeginTransaction())
            {
                var mgr = CreateLargeObjectManager();
                uint oid = mgr.Create();
                using (Stream stream = mgr.OpenReadWrite(oid))
                {
                    await streamWriter(stream);
                }
                TEntity binary = await entitySupplier();
                if (binary != null)
                {
                    binary.ObjectId = oid;
                    this.Add(binary);
                    this.SaveChanges();
                    trans.Commit();
                }
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
                trans.Commit();
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
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ReadLargeObject(binary, stream);   
                }
            }
            return binary;
        }

        private void WriteLargeObject<TEntity>(TEntity binary, Stream input) where TEntity : Binary
        {
            var mgr = CreateLargeObjectManager();
            uint oid = mgr.Create();
            using (var stream = mgr.OpenReadWrite(oid))
            {
                input.CopyTo(stream);
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
            builder.Entity<Binary>().HasDiscriminator<String>("Type")
                                    .HasValue<Image>("Image");
            // shadow properties
            builder.Entity<Image>().Property<Nullable<DateTime>>("UpdateTime");
            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            this.ChangeTracker.DetectChanges();
            UpdateProperties<Image>();
            // TODO: make sure that all derived types are added here
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
