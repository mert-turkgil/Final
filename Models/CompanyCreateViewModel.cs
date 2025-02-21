using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Final.Entity;

namespace Final.Models
{
    public class CompanyCreateViewModel
    {
        #nullable disable
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string BaseTopic { get; set; }
        
        // A sample topic template (e.g.: "pwr_rqst/room{room}/control_room/{BaseTopic}")
        public string TopicTemplate { get; set; }
        
        // A numeric value used in the template (e.g.: how many items or a sample room number)
        public int HowMany { get; set; }
        
        // The expected data type of the topic value (to later help determine sample values)
        public TopicDataType DataType { get; set; }
        public string ToolBaseTopic { get; set; }

        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string ToolName { get; set; }
    }
}