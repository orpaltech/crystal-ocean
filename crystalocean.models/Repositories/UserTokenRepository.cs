using System;
using System.Linq;
using System.Threading.Tasks;
using CrystalOcean.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CrystalOcean.Data.Repository
{
    public class UserTokenRepository : DbContext
    {
        public UserTokenRepository(DbContextOptions<UserTokenRepository> options) 
            : base(options)
        {
        }

        public Task<UserToken> FindOneAsync(long userId, String id) 
        {
            return Tokens.Where(t => t.UserId == userId && t.Id == id).FirstOrDefaultAsync();
        }

        public DbSet<UserToken> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UserToken>().HasKey(m => m.Id);
            // shadow properties
            builder.Entity<UserToken>().Property<Nullable<DateTime>>("UpdateTime");
            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            UpdateProperties<UserToken>();
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
    }
}