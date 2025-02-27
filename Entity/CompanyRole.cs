using System;
using System.ComponentModel.DataAnnotations;
using Final.Identity;

namespace Final.Entity
{
    public class CompanyRole
    {
        #nullable disable
        [Key]
        public int Id { get; set; }

        public Guid CompanyId { get; set; }
        public virtual Company Company { get; set; }

        [Required]
        public string RoleId { get; set; }

        // Navigation property – used by ApplicationDbContext for relationship mapping.
        public virtual ApplicationRole Role { get; set; }
    }
}
