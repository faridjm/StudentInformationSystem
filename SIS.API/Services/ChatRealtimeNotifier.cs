using Microsoft.AspNetCore.SignalR;
using SIS.API.Hubs;
using SIS.BLL.Services.DTOs.Chat;
using SIS.BLL.Services.Interfaces;

namespace SIS.API.Services
{
    /// <summary>
    /// ChatHub-a fan-out edən SignalR notifier — REST endpoint vasitəsilə
    /// göndərilən mesajları da real-time olaraq qarşı tərəfə çatdırır.
    /// </summary>
    public class ChatRealtimeNotifier : IChatRealtimeNotifier
    {
        private readonly IHubContext<ChatHub> _hub;
        public ChatRealtimeNotifier(IHubContext<ChatHub> hub) { _hub = hub; }

        public Task NotifyMessageAsync(int conversationId, MessageDto message)
            => _hub.Clients.Group($"conversation-{conversationId}")
                .SendAsync("ReceiveMessage", message);

        public Task NotifyMessagesReadAsync(int conversationId, string readerUserId)
            => _hub.Clients.Group($"conversation-{conversationId}")
                .SendAsync("MessagesRead", conversationId, readerUserId);
    }
}
