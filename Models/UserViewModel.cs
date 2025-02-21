using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Final.Identity;
using Microsoft.AspNetCore.Identity;

namespace Final.Models
{
    public class UserViewModel
    {
        #nullable disable
        public string Id { get; set; }  
        // User properties
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // Companies with their tools and topics (e.g., rooms)
        public List<CompanyDto> Companies { get; set; } = new List<CompanyDto>();
        // Role management data
        public List<IdentityRole> Roles { get; set; } = new List<IdentityRole>();
    }
        public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string BaseTopic { get; set; }
        
        // Tools belonging to this company
        public List<ToolDto> Tools { get; set; } = new List<ToolDto>();
        public List<string> RoleIds { get; set; } = new List<string>();

    }

    public class ToolDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        // Topics associated with this tool
        public List<TopicDto> Topics { get; set; } = new List<TopicDto>();
    }

    public class TopicDto
    {
        public Guid Id { get; set; }
        public string BaseTopic { get; set; }
        public string TopicTemplate { get; set; }
        public int HowMany { get; set; }
        public string DataType { get; set; }
    }
}