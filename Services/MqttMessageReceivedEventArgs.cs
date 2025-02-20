using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Final.Services
{
    public class MqttMessageReceivedEventArgs : EventArgs
    {
        public string Topic { get; }
        public string Payload { get; }

        public MqttMessageReceivedEventArgs(string topic, string payload)
        {
            Topic = topic;
            Payload = payload;
        }
    }
}