using Korik.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Korik.Infrastructure
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly IUserConnectionManager _connectionManager;

      public NotificationHub(IUserConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _connectionManager.AddConnection(userId, Context.ConnectionId);
                Console.WriteLine($"User {userId} connected with ConnectionId: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
             _connectionManager.RemoveConnection(Context.ConnectionId);
             Console.WriteLine($"ConnectionId {Context.ConnectionId} disconnected");

            await base.OnDisconnectedAsync(exception);
        }
    }
}
