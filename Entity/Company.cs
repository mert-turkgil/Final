using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Final.Entity
{
    public class Company
    {
        #nullable disable
        [Key]
        public Guid Id { get; set; }
        
        // The name of the company (also can serve as a base topic prefix)
        [Required]
        public string Name { get; set; }
        
        // The base topic for the company, e.g. "ciceklisogukhavadeposu"
        [Required]
        public string BaseTopic { get; set; }
        
        // Navigation property for tools that belong to the company
        public virtual ICollection<MqttTool> Tools { get; set; } = new List<MqttTool>();
        // Navigation property for topic that belong to the company
        public virtual ICollection<MqttTopic> Topics { get; set; } = new List<MqttTopic>();
        // Navigation property for roles bound to the company
        public virtual ICollection<CompanyRole> CompanyRoles { get; set; } = new List<CompanyRole>();
    }
}