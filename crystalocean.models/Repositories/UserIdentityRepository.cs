using CrystalOcean.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrystalOcean.Data.Repository
{
    public class UserIdentityRepository : IdentityDbContext<User, Role, long>
    {
        public UserIdentityRepository(DbContextOptions<UserIdentityRepository> options) 
            : base (options) 
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Role>().ToTable("Role");
            modelBuilder.Entity<IdentityUserClaim<long>>().ToTable("UserClaim");
            modelBuilder.Entity<IdentityRoleClaim<long>>().ToTable("RoleClaim");
            modelBuilder.Entity<IdentityUserRole<long>>().ToTable("UserRole");
            modelBuilder.Entity<IdentityUserLogin<long>>().ToTable("ExternalLogin");

        }
    }
}