using Microsoft.AspNetCore.SignalR;
using SIS.API.Hubs;
using SIS.BLL.Services.Interfaces;

namespace SIS.API.Services
{
    public class SignalRNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotifier(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyUserAsync(string userId, string title, string message, string type)
        {
            await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
