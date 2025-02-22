using System.ComponentModel.DataAnnotations;
using Final.Entity;

namespace Final.Models
{
    public class SendingTopicViewModel
    {
        #nullable disable
        [Required]
        public string TopicTemplate { get; set; }
        
        // Number of topics to generate using this template.
        [Required]
        public int HowMany { get; set; }
        
        // The expected data type for this sending topic.
        [Required]
        public TopicDataType DataType { get; set; }
    }
}
