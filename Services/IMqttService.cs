using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Final.Services
{
    public interface IMqttService
    {
        /// <summary>
        /// Connects to the MQTT broker.
        /// </summary>
         Task ConnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Disconnects from the MQTT broker.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Subscribes to the given topic.
        /// </summary>
        Task SubscribeAsync(string topic);

        /// <summary>
        /// Publishes a message with a generic payload to the given topic.
        /// </summary>
        Task PublishAsync(string topic, object payload);

        /// <summary>
        /// Occurs when an MQTT message is received.
        /// </summary>
        event EventHandler<MqttMessageReceivedEventArgs> MessageReceived;
    }
}