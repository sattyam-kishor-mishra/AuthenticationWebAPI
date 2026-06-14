using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutenticationWeb.API
{

    public class ApplicationUser : IdentityUser
    {
    }

    public class ApplicationRole : IdentityRole
    {        
    }

    public class ApplicationDbContext: IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        }

    }
}
