using Final.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Final.Identity
{
    public class ApplicationDbContext : IdentityDbContext<User, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // If needed, you can add additional domain DbSets here.

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Let Identity configure its tables first.
            base.OnModelCreating(builder);

            // Configure the relationship: each CompanyRole must reference an ApplicationRole.
            builder.Entity<CompanyRole>()
                .HasOne(cr => cr.Role)
                .WithMany(r => r.CompanyRoles)
                .HasForeignKey(cr => cr.RoleId)
                .IsRequired();
        }
    }
}
