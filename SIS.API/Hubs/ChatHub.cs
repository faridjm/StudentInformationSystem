using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SIS.BLL.Services.Interfaces;

namespace SIS.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService) { _chatService = chatService; }

        private string UserId => Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

        public async Task JoinConversation(int conversationId)
        {
            if (!await _chatService.HasAccessAsync(conversationId, UserId))
                throw new HubException("Bu söhbətə giriş icazəniz yoxdur");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
            await _chatService.MarkAsReadAsync(conversationId, UserId);
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
        }

        public async Task SendMessage(int conversationId, string content)
        {
            if (!await _chatService.HasAccessAsync(conversationId, UserId))
                throw new HubException("Bu söhbətə giriş icazəniz yoxdur");

            var message = await _chatService.SendMessageAsync(UserId, conversationId, content);

            await Clients.Group($"conversation-{conversationId}")
                .SendAsync("ReceiveMessage", message);
        }

        public async Task MarkAsRead(int conversationId)
        {
            if (!await _chatService.HasAccessAsync(conversationId, UserId))
                return;

            await _chatService.MarkAsReadAsync(conversationId, UserId);
            await Clients.Group($"conversation-{conversationId}")
                .SendAsync("MessagesRead", conversationId, UserId);
        }
    }
}
