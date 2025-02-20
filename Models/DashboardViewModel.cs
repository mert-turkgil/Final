using Final.Entity;
using System.Collections.Generic;

namespace Final.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<Company> Companies { get; set; } = new List<Company>();
        public IEnumerable<MqttTool> MqttTools { get; set; } = new List<MqttTool>();
        public IEnumerable<MqttTopic> MqttTopics { get; set; } = new List<MqttTopic>();
    }
}
