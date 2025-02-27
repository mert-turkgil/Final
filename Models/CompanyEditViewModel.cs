using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Final.Entity;

namespace Final.Models
{
    #nullable disable
    public class CompanyEditViewModel
    {
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string BaseTopic { get; set; }
        
        // Company-level Topic Templates for subscription or sending.
        // You can have multiple templates with different layouts.
        public List<EditCompanyTopicTemplateViewModel> CompanyTopicTemplates { get; set; } = new List<EditCompanyTopicTemplateViewModel>();

        // Alternatively, if you wish to differentiate sending topics, you can add:
        public List<EditCompanyTopicTemplateViewModel> SendingTopics { get; set; } = new List<EditCompanyTopicTemplateViewModel>();

        // Tools for the company.
        public List<ToolEditViewModel> Tools { get; set; } = new List<ToolEditViewModel>();
    }

    public class EditCompanyTopicTemplateViewModel
    {
        [Required]
        public int Id { get; set; }
        /// <summary>
        /// The topic pattern that can contain a placeholder {seq} which will be replaced
        /// sequentially from 1 to HowMany. For example: 
        /// "ciceklisogukhavadeposu/control_room/room{seq}/temp"
        /// </summary>
        [Required]
        public string Template { get; set; }
        
        /// <summary>
        /// The number of sequential topics to generate based on this template.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "HowMany must be greater than zero.")]
        public int HowMany { get; set; }
        
        [Required]
        public TopicDataType DataType { get; set; }
        
        [Required]
        public TopicPurpose TopicPurpose { get; set; }
    }

    public class ToolEditViewModel
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string ToolName { get; set; }
        
        [Required]
        public string ToolBaseTopic { get; set; }
        
        public string Description { get; set; }
        
        public string ImageUrl { get; set; }
        // New property for file upload:
        public IFormFile ImageFile { get; set; }
        
        // Topic Templates for this tool.
        public List<EditToolTopicTemplateViewModel> TopicTemplates { get; set; } = new List<EditToolTopicTemplateViewModel>();
    }
    
    public class EditToolTopicTemplateViewModel
    {
        /// <summary>
        /// The topic pattern for this tool. It can include a placeholder {seq}
        /// which will be replaced sequentially from 1 to HowMany.
        /// For example: "ciceklisogukhavadeposu/tool/{seq}/status"
        /// </summary>
        [Required]
        public string Template { get; set; }
        
        /// <summary>
        /// The number of sequential topics to generate based on this template.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "HowMany must be greater than zero.")]
        public int HowMany { get; set; }
        
        [Required]
        public TopicDataType DataType { get; set; }
        
        [Required]
        public TopicPurpose TopicPurpose { get; set; }
    }
}
