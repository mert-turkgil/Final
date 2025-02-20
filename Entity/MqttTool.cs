using System.ComponentModel.DataAnnotations;

namespace Final.Entity
{
    public class MqttTool
    {
        #nullable disable
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
        // Optionally, you might store a company-specific prefix or override here.
        public string ToolBaseTopic { get; set; }

        public string Description { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        public Guid CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public virtual ICollection<MqttTopic> Topics { get; set; } = new List<MqttTopic>();
    }
}