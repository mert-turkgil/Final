using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Final.Entity
{
    public class CompanyRole
    {
        #nullable disable
        [Key]
        public int Id { get; set; }

        public Guid CompanyId { get; set; }
        public virtual Company Company { get; set; }  // Must be virtual

        [Required]
        public string RoleId { get; set; }

        // Mark this property as virtual
        public virtual IdentityRole Role { get; set; }
    }
}
