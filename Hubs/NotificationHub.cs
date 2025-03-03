using Final.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Final.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly IMqttService _mqttService;
        private readonly IMqttLogService _mqttLogService;
        public NotificationHub(IMqttService mqttService,IMqttLogService mqttLogService)
        {
            _mqttService = mqttService;
            _mqttLogService = mqttLogService;
        }
        public override async Task OnConnectedAsync()
        {
            // Retrieve the company identifier from the user's claims.
            // For example, assume a claim named "company" holds the company base topic.
            string? company = Context.User?.FindFirst("company")?.Value;
            if (!string.IsNullOrEmpty(company))
            {
                // Add this connection to a group named after the company.
                await Groups.AddToGroupAsync(Context.ConnectionId, company);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Remove connection from the company group on disconnect.
            string? company = Context.User?.FindFirst("company")?.Value;
            if (!string.IsNullOrEmpty(company))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, company);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Optional: Allow clients to join additional groups if needed.
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("ReceiveNotification", $"Joined group: {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("ReceiveNotification", $"Left group: {groupName}");
        }

        public async Task SendLog(string message)
        {
            await Clients.All.SendAsync("ReceiveSubscriptionLog", message);
        }
        /// <summary>
        /// Publish a message to an MQTT topic and log it.
        /// </summary>
        /// <param name="topic">The MQTT topic to publish to.</param>
        /// <param name="payload">The message payload.</param>
        /// <param name="companyId">Optional: If you have a specific company to associate with this publish.</param>
        public async Task PublishMessage(string topic, string payload, Guid? companyId = null)
        {
            // 1) Publish to MQTT
            await _mqttService.PublishAsync(topic, payload);

            // 2) Add a log entry so it appears in the same logs as other MQTT actions.
            // If you have a specific company, use its ID and name. Otherwise, use a generic placeholder.
            var logCompanyId = companyId ?? Guid.Empty;
            var companyName = companyId.HasValue ? "SomeCompanyName" : "MQTTPublisher";

            // Add a log entry that includes the topic and payload
            _mqttLogService.AddLog(logCompanyId, companyName, $"[Publish] topic: {topic} => payload: {payload}");

            // 3) Also broadcast to all clients that a message was published
            await Clients.All.SendAsync("ReceiveSubscriptionLog", $"[Publish] => {topic} => {payload}");
        }
    }
}
