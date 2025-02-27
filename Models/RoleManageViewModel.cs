using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Final.Identity;
using Microsoft.AspNetCore.Identity;

namespace Final.Models
{
    public class RoleManageViewModel
    {
        public List<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();
        public List<CompanyDto> Companies { get; set; } = new List<CompanyDto>();
    }
}