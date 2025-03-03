using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Final.Entity;

namespace Final.Models.ViewComponents
{
    #nullable disable
    public class CompanyDashboardViewModel
    {
        // A collection of companies to display.
        public List<CompanyViewModel> Companies { get; set; } = new List<CompanyViewModel>();
    }
    
    public class CompanyViewModel
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string BaseTopic { get; set; }
        
        // Company-level sending topics.
        public List<TopicViewModel> CompanyTopics { get; set; } = new List<TopicViewModel>();
        
        // Tools belonging to this company.
        public List<ToolViewModel> Tools { get; set; } = new List<ToolViewModel>();

        // List of role IDs (names) associated with the company.
        public List<string> RoleIds { get; set; } = new List<string>();
    }
    
    public class ToolViewModel
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string ToolBaseTopic { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        
        public List<TopicViewModel> Topics { get; set; } = new List<TopicViewModel>();
    }
    
    public class TopicViewModel
    {
        public Guid Id { get; set; }
        
        [Required]
        public string TopicTemplate { get; set; }
        
        public int HowMany { get; set; }
        
        [Required]
        public string DataType { get; set; }
        
        // Indicates whether this topic is for Subscription or Sending.
        public TopicPurpose Purpose { get; set; }
    }
}
