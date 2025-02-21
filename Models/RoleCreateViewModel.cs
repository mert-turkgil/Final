using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Final.Models
{
    public class RoleCreateViewModel
    {
        #nullable disable
        [Required]
        public string RoleName { get; set; }
        [Required]
        public Guid CompanyId { get; set; }
        // Optionally assign users on creation
        public List<string> SelectedUserIds { get; set; } = new List<string>();

        public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();
    }
}