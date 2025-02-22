using System;
using System.ComponentModel.DataAnnotations;
using Final.Entity;

namespace Final.Entity
{
    public class MqttTopic
    {
        #nullable disable
        [Key]
        public Guid Id { get; set; }
        
        // Base topic, e.g., "ciceklisogukhavadeposu"
        [Required]
        public string BaseTopic { get; set; }

        // Full topic template, e.g. "ciceklisogukhavadeposu/control_room/room{room}/status"
        [Required]
        public string TopicTemplate { get; set; }
        [Required]
        public int HowMany { get; set; }

        // The data type of the topic value (e.g., Int64, Float)
        [Required]
        public TopicDataType DataType { get; set; }

        // Distinguish between subscription and sending topics.
        [Required]
        public TopicPurpose TopicPurpose { get; set; }
        // Optional relationship to a tool that owns this topic
        public Guid? MqttToolId { get; set; }
        public virtual MqttTool MqttTool { get; set; }
        public byte[] Data64 { get; set; } = new byte[64];

    }
}
