using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrystalOcean.Data.Models.Repository
{
    public class BinaryRepository : DbContext
    {
        public BinaryRepository(DbContextOptions<BinaryRepository> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Binary>().HasKey(m => m.Id);
            // shadow properties
            builder.Entity<Binary>().Property<Nullable<DateTime>>("UpdateTime");
 
            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();

            return base.SaveChanges();
        }
    }
}
