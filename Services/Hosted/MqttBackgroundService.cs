using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Final.Data.Abstract;
using Final.Entity;
using Final.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Final.Services.Hosted
{
    public class MqttBackgroundService : IHostedService, IDisposable
    {
        private readonly IMqttService _mqttService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<MqttBackgroundService> _logger;
        private CancellationTokenSource? _cts;

        public MqttBackgroundService(
            IMqttService mqttService,
            IServiceProvider serviceProvider,  // Used to create scopes
            IHubContext<NotificationHub> hubContext,
            ILogger<MqttBackgroundService> logger)
        {
            _mqttService = mqttService;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MQTT Background Service");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _mqttService.MessageReceived += OnMqttMessageReceived;

            await _mqttService.ConnectAsync();

            // Create a scope to retrieve companies from the database.
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IShopUnitOfWork>();
                var companies = (await unitOfWork.CompanyRepository.GetAllAsync()).ToList();

                foreach (var company in companies)
                {
                    _logger.LogInformation($"Subscribing for company: {company.Name}, BaseTopic: {company.BaseTopic}");

                    foreach (var tool in company.Tools)
                    {
                        foreach (var topic in tool.Topics)
                        {

                            string topicToSubscribe = topic.TopicTemplate;
                            _logger.LogInformation($"Subscribing to topic: {topicToSubscribe}");
                            await _hubContext.Clients.Group(company.BaseTopic)
                            .SendAsync("ReceiveSubscriptionLog", $"[>>{DateTime.Now:HH:mm:ss}] [Info] Company {company.Id}: Subscribing to topic: {topicToSubscribe}");
                            await _mqttService.SubscribeAsync(topicToSubscribe);
                        }
                    }
                }
            }
        }

        private async void OnMqttMessageReceived(object? sender, MqttMessageReceivedEventArgs e)
        {
            _logger.LogInformation($"MQTT message received: Topic: {e.Topic}, Payload: {e.Payload}");

            // Create a new scope to resolve IShopUnitOfWork
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IShopUnitOfWork>();
                var companies = await unitOfWork.CompanyRepository.GetAllAsync();
                var company = companies.FirstOrDefault(c => e.Topic.Contains(c.BaseTopic, StringComparison.OrdinalIgnoreCase));
                if (company != null)
                {
                    // Use the company's BaseTopic as the SignalR group name.
                    string groupName = company.BaseTopic;
                    await _hubContext.Clients.Group(company.BaseTopic)
                            .SendAsync("ReceiveSubscriptionLog", $"[{DateTime.Now:HH:mm:ss}] [Info] Company {company.Id}- ReceiveMqttMessage--->> {e.Topic} --> {e.Payload}");
                    await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMqttMessage", e.Topic, e.Payload);
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
