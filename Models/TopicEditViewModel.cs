using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Final.Entity;

namespace Final.Models
{
    public class TopicEditViewModel
    {
        #nullable disable
        
        [Required]
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Base topic is required.")]
        [Display(Name = "Base Topic")]
        public string BaseTopic { get; set; }
        
        [Required(ErrorMessage = "Topic template is required.")]
        [Display(Name = "Topic Template")]
        public string TopicTemplate { get; set; }
        
        [Required]
        [Display(Name = "How Many")]
        public int HowMany { get; set; }
        
        [Required]
        [Display(Name = "Data Type")]
        public TopicDataType DataType { get; set; }
        
        [Required]
        [Display(Name = "Topic Purpose")]
        public TopicPurpose TopicPurpose { get; set; }
    }
}