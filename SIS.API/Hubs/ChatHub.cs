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
            // Səhifə açıldıqda mövcud unread mesajları read et — diqqət: bu MessagesRead event-ini fan-out edir
            await _chatService.MarkAsReadAsync(conversationId, UserId);
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
        }

        public async Task MarkAsRead(int conversationId)
        {
            if (!await _chatService.HasAccessAsync(conversationId, UserId))
                return;
            await _chatService.MarkAsReadAsync(conversationId, UserId);
        }
    }
}
