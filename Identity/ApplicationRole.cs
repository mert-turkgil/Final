using Final.Entity;
using Microsoft.AspNetCore.Identity;

namespace Final.Identity
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }

        // Add this navigation property:
        public virtual ICollection<CompanyRole> CompanyRoles { get; set; } = new List<CompanyRole>();
    }
}
