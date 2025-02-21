using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Final.Models
{
    public class RoleEditViewModel
    {
        #nullable disable
        [Key]
        public string Id { get; set; }
        public string RoleName { get; set; }
        public Guid CompanyId { get; set; }
        public List<string> SelectedUserIds { get; set; } = new List<string>();

        public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();
    }
}