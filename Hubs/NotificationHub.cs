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
    }
}
