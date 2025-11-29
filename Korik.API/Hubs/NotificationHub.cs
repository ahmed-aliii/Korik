using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Korik.API
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to role-based groups for easier broadcasting
                var roles = Context.User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                if (roles != null)
                {
                    foreach (var role in roles)
                    {
                        if (role == "Workshop")
                        {
                            await Groups.AddToGroupAsync(Context.ConnectionId, "Workshops");
                        }
                        else if (role == "CarOwner")
                        {
                            await Groups.AddToGroupAsync(Context.ConnectionId, "CarOwners");
                        }
                    }
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Remove user from role-based groups
                var roles = Context.User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                if (roles != null)
                {
                    foreach (var role in roles)
                    {
                        if (role == "Workshop")
                        {
                            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Workshops");
                        }
                        else if (role == "CarOwner")
                        {
                            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "CarOwners");
                        }
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Optional: Allow clients to manually mark notifications as read
        public async Task MarkNotificationAsRead(int notificationId)
        {
            // This can be called from the client to mark a notification as read
            // You would implement this logic in a separate service
            await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
        }
    }
}
