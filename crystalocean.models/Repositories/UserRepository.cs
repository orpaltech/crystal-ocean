using System;
using System.Linq;
using CrystalOcean.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrystalOcean.Data.Repository
{
    public class UserRepository : DbContext
    {
        public UserRepository(DbContextOptions<UserRepository> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>().HasKey(m => m.Id);
            // shadow properties
            builder.Entity<User>().Property<Nullable<DateTime>>("UpdateTime");
            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            UpdateProperties<User>();
            return base.SaveChanges();
        }
 
        private void UpdateProperties<T>() where T : class
        {
            var modifiedSourceInfo =
                ChangeTracker.Entries<T>()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
 
            foreach (var entry in modifiedSourceInfo)
            {
                entry.Property("UpdateTime").CurrentValue = DateTime.UtcNow;
            }
        }

        public DbSet<User> Users { get;set; }
    }
}
