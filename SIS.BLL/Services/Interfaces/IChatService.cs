using SIS.BLL.Services.DTOs.Chat;

namespace SIS.BLL.Services.Interfaces
{
    public interface IChatService
    {
        Task<IEnumerable<ConversationDto>> GetMyConversationsAsync(string userId);
        Task<ConversationDto> GetOrCreateConversationAsync(string currentUserId, int groupId, string targetUserId);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, string currentUserId, int take = 50, int skip = 0);
        Task<MessageDto> SendMessageAsync(string senderUserId, int conversationId, string content);
        Task<MessageDto> SendAttachmentMessageAsync(string senderUserId, SendAttachmentMessageDto dto);
        Task MarkAsReadAsync(int conversationId, string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> HasAccessAsync(int conversationId, string userId);
    }
}
