using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Final.Data.Abstract;
using Final.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Final.Services.Hosted
{
    public class MqttBackgroundService : IHostedService, IDisposable
    {
        private readonly IMqttService _mqttService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MqttBackgroundService> _logger;
        private readonly IMqttLogService _mqttLogService;
        private CancellationTokenSource? _cts;

        public MqttBackgroundService(
            IMqttService mqttService,
            IServiceProvider serviceProvider,
            ILogger<MqttBackgroundService> logger,
            IMqttLogService mqttLogService)
        {
            _mqttService = mqttService;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _mqttLogService = mqttLogService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MQTT Background Service");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Hook up our message handler.
            _mqttService.MessageReceived += OnMqttMessageReceived;

            // Connect to the MQTT broker.
            await _mqttService.ConnectAsync();

            // Create a scope to retrieve data from the database.
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IShopUnitOfWork>();
                var companies = (await unitOfWork.CompanyRepository.GetAllAsync()).ToList();

                foreach (var company in companies)
                        {
                            // 1) Gather all subscription topics for this company:
                            //    a) Company-level topics
                            var companyTopics = await unitOfWork.MqttTopicRepository
                                .GetByCompanyIdAndNoToolAsync(company.Id);

                            //    b) Tool-level topics
                            var toolTopics = company.Tools
                                .SelectMany(tool => tool.Topics)
                                .Where(t => t.TopicPurpose == TopicPurpose.Subscription)
                                .ToList();

                            // Combine them
                            var allTopics = companyTopics
                                .Where(t => t.TopicPurpose == TopicPurpose.Subscription)
                                .Concat(toolTopics)
                                .ToList();

                            // 2) Sort by room number
                            var sortedTopics = allTopics
                                .OrderBy(t => ExtractRoomNumber(t.TopicTemplate))
                                .ThenBy(t => t.TopicTemplate) // (Optional secondary sort)
                                .ToList();

                            // 3) Subscribe in ascending order
                            foreach (var topic in sortedTopics)
                            {
                                string topicToSubscribe = topic.TopicTemplate;
                                string logMessage = $"Subscribing -> : {topicToSubscribe}";
                                _mqttLogService.AddLog(company.Id, company.Name, logMessage);
                                await _mqttService.SubscribeAsync(topicToSubscribe);
                            }
                         }
            }
        }

        private async void OnMqttMessageReceived(object? sender, MqttMessageReceivedEventArgs e)
        {
            _logger.LogInformation($"MQTT - Topic: {e.Topic}, Payload: {e.Payload}");

            // Create a scope to resolve IShopUnitOfWork.
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IShopUnitOfWork>();
                var companies = await unitOfWork.CompanyRepository.GetAllAsync();

                // Loop through each company.
                foreach (var company in companies)
                {
                    // If the incoming topic contains the company's BaseTopic, assume it's for that company.
                    if (e.Topic.Contains(company.BaseTopic, StringComparison.OrdinalIgnoreCase))
                    {
                        // If the topic appears to be related to a room (contains "room")
                        if (e.Topic.Contains("room", StringComparison.OrdinalIgnoreCase))
                        {
                            // Extract the room number (implement your own extraction logic)
                            int roomNumber = ExtractRoomNumber(e.Topic);
                            
                            // Retrieve or create a generic card for this room.
                            var card = LiveDataStore.LiveRoomData.GetOrAdd(roomNumber, new Final.Models.LiveData.GenericCardModel
                            {
                                Id = roomNumber,
                                CardName = $"Room {roomNumber}",
                                Description = "Live room data",
                                LastUpdated = DateTime.UtcNow
                            });
                            
                            // Update a metric for the room; for example, store the payload under "Value".
                            card.Metrics["Value"] = e.Payload;
                            card.LastUpdated = DateTime.UtcNow;
                        }
                        else
                        {
                            // Otherwise, assume it's a tool message.
                            // Look through the company's tools to see if the topic matches a tool's base topic.
                            foreach (var tool in company.Tools)
                            {
                                if (e.Topic.Contains(tool.ToolBaseTopic, StringComparison.OrdinalIgnoreCase))
                                {
                                    // Use a unique key for the tool (for example, the hash of its GUID).
                                    int toolKey = tool.Id.GetHashCode();
                                    
                                    // Retrieve or create a generic tool model.
                                    var genericTool = LiveDataStore.ToolData.GetOrAdd(toolKey, new Final.Models.LiveData.GenericToolModel
                                    {
                                        Id = toolKey,
                                        ToolId = tool.Id,
                                        Name = tool.Name,
                                        ToolBaseTopic = tool.ToolBaseTopic,
                                        Description = tool.Description,
                                        ImageUrl = tool.ImageUrl,
                                        CompanyId = company.Id,
                                    });
                                    
                                    // Update live values for the tool (for example, "Status").
                                    genericTool.LiveValues["Status"] = e.Payload;
                                }
                            }
                        }

                        // Log the message for the matching company.
                        string logMessage = $"Received: {e.Topic} -> {e.Payload}";
                        _mqttLogService.AddLog(company.Id, company.Name, logMessage);
                        _logger.LogInformation(logMessage);
                    }
                }
            }
        }

        // Example helper: implement your own room number extraction logic based on topic naming.
        private int ExtractRoomNumber(string topic)
        {
            // For instance, if the topic contains "room" followed by a number, extract it.
            // This is a simple placeholder implementation.
            var parts = topic.Split(new char[] { '/', '_' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (part.StartsWith("room", StringComparison.OrdinalIgnoreCase))
                {
                    string numberPart = part.Substring(4); // Remove "room"
                    if (int.TryParse(numberPart, out int roomNumber))
                    {
                        return roomNumber;
                    }
                }
            }
            // Default if extraction fails.
            return 0;
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping MQTT Background Service");
            _mqttService.MessageReceived -= OnMqttMessageReceived;
            _cts?.Cancel();
            await _mqttService.DisconnectAsync();
        }

        public void Dispose()
        {
            _cts?.Dispose();
        }
    }
}
