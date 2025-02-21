using Microsoft.AspNetCore.Identity;

namespace Final.Identity
{
    public class ApplicationRole : IdentityRole
    {
        // Add any custom properties if needed.
        // Ensure this class is public, unsealed, and has a public or protected constructor.
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
    }
}
