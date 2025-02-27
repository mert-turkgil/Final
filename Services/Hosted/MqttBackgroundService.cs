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
                    _logger.LogInformation($"Subscribing for company: {company.Name}");

                    // 1. Subscribe to company-level topics (where MqttToolId == null),
                    //    but only those with TopicPurpose == Subscription.
                    var companyTopics = await unitOfWork.MqttTopicRepository.GetByCompanyIdAndNoToolAsync(company.Id);
                    foreach (var topic in companyTopics.Where(t => t.TopicPurpose == TopicPurpose.Subscription))
                    {
                        string topicToSubscribe = topic.TopicTemplate;
                        string logMessage = $"Subscribing to company topic: {topicToSubscribe}";
                        _mqttLogService.AddLog(company.Id, company.Name, logMessage);
                        _logger.LogInformation(logMessage);
                        await _mqttService.SubscribeAsync(topicToSubscribe);
                    }

                    // 2. Subscribe to each tool's topics, but only if they are for Subscription.
                    foreach (var tool in company.Tools)
                    {
                        foreach (var topic in tool.Topics.Where(t => t.TopicPurpose == TopicPurpose.Subscription))
                        {
                            string topicToSubscribe = topic.TopicTemplate;
                            string logMessage = $"Subscribing to tool topic: {topicToSubscribe}";
                            _mqttLogService.AddLog(company.Id, company.Name, logMessage);
                            _logger.LogInformation(logMessage);
                            await _mqttService.SubscribeAsync(topicToSubscribe);
                        }
                    }
                }
            }
        }

        private async void OnMqttMessageReceived(object? sender, MqttMessageReceivedEventArgs e)
        {
            _logger.LogInformation($"MQTT - Topic: {e.Topic}, Payload: {e.Payload}");

            // Create a scope to resolve IShopUnitOfWork and log the incoming message.
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IShopUnitOfWork>();
                var companies = await unitOfWork.CompanyRepository.GetAllAsync();

                // Find which company this topic might belong to by matching BaseTopic.
                var company = companies.FirstOrDefault(c => 
                    e.Topic.Contains(c.BaseTopic, StringComparison.OrdinalIgnoreCase));

                if (company != null)
                {
                    string message = $"Received: {e.Topic} -> {e.Payload}";
                    _mqttLogService.AddLog(company.Id, company.Name, message);
                    _logger.LogInformation(message);
                }
                else
                {
                    _logger.LogWarning("Received message for unknown company.");
                }
            }
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
