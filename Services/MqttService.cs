using System;
using System.Text;
using System.Threading.Tasks;
using Final.Configuration;
using Final.Services;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace Final.Services
{
    public class MqttService : IMqttService
    {
        private readonly IMqttClient _mqttClient;
        private readonly IMqttClientOptions _mqttOptions;

        public event EventHandler<MqttMessageReceivedEventArgs> MessageReceived = delegate { };

        public MqttService(IOptions<MqttConfig> config)
        {
            var mqttConfig = config.Value;
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            _mqttClient.UseApplicationMessageReceivedHandler(OnApplicationMessageReceived);

            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId(mqttConfig.ClientId)
                .WithTcpServer(mqttConfig.Host, mqttConfig.Port)
                .WithCredentials(mqttConfig.Username, mqttConfig.Password)
                .WithCleanSession()
                .Build();
        }

        public async Task ConnectAsync()
        {
            if (!_mqttClient.IsConnected)
            {
                await _mqttClient.ConnectAsync(_mqttOptions);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }
        }

        public async Task SubscribeAsync(string topic)
        {
            if (!_mqttClient.IsConnected)
            {
                await ConnectAsync();
            }

            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build();
            
            await _mqttClient.SubscribeAsync(topicFilter);
        }

        public async Task PublishAsync(string topic, object payload)
        {
            if (!_mqttClient.IsConnected)
            {
                await ConnectAsync();
            }

            // Convert the payload to string representation.
            // This method can be extended to handle binary or other data types.
            string messagePayload = ConvertPayloadToString(payload);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(messagePayload)
                .WithAtLeastOnceQoS()
                .Build();

            await _mqttClient.PublishAsync(message);
        }

        private string ConvertPayloadToString(object payload)
        {
            // Generic conversion logic.
            // Extend this method to handle additional types or custom serialization if needed.
            return payload switch
            {
                null => string.Empty,
                float f => f.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                double d => d.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                int i => i.ToString(),
                long l => l.ToString(),
                byte[] bytes => Convert.ToBase64String(bytes),
                _ => payload.ToString() ?? string.Empty
            };
        }

        private void OnApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            // Convert the payload from bytes to a string.
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            // Raise the MessageReceived event.
            MessageReceived?.Invoke(this, new MqttMessageReceivedEventArgs(e.ApplicationMessage.Topic, payload));
        }
    }
}
