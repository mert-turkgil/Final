using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Final.Configuration
{
    public class MqttConfig
    {
        public string Host { get; set; } = "test.mosquitto.org";
        public int Port { get; set; } = 1883;
        public string ClientId { get; set; } = "MyMqttClient";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
