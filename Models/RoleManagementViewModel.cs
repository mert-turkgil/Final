using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Final.Models
{
    public class RoleManagementViewModel
    {
        public List<IdentityRole> Roles { get; set; } = new List<IdentityRole>();
        public List<CompanyDto> Companies { get; set; } = new List<CompanyDto>();
    }
}