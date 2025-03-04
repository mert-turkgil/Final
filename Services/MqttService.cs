using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Final.Configuration;
using Final.Models;
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
        private readonly ConcurrentDictionary<Guid, List<MqttLog>> _logs = new ConcurrentDictionary<Guid, List<MqttLog>>();
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

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _mqttClient.ConnectAsync(_mqttOptions, cancellationToken);
                Console.WriteLine("Connected to MQTT broker successfully.");
            }
            catch (Exception ex)
            {
                // Log the error with details
                Console.WriteLine($"Error connecting to MQTT broker: {ex.Message}");
                // Optionally, implement retry logic or switch broker here.
                throw;
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
            
            // The logging and SignalR messaging should be done in your background service.
            await _mqttClient.SubscribeAsync(topicFilter);
        }

        public async Task PublishAsync(string topic, object payload)
        {
            if (!_mqttClient.IsConnected)
            {
                await ConnectAsync();
            }

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
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            MessageReceived?.Invoke(this, new MqttMessageReceivedEventArgs(e.ApplicationMessage.Topic, payload));
        }
        public List<MqttLog> GetAllLogs()
        {
            // Aggregate logs from all companies.
            return _logs.Values.SelectMany(logList => logList).OrderBy(log => log.Timestamp).ToList();
        }

    }
}
