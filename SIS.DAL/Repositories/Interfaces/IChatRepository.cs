using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatConversation?> GetConversationAsync(int conversationId);
        Task<ChatConversation?> FindConversationAsync(int groupId, string teacherUserId, string studentUserId);
        Task<IEnumerable<ChatConversation>> GetConversationsByUserAsync(string userId);
        Task<ChatConversation> CreateConversationAsync(ChatConversation conversation);
        Task<ChatMessage> AddMessageAsync(ChatMessage message);
        Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId, int take = 50, int skip = 0);
        Task MarkAsReadAsync(int conversationId, string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<int> GetUnreadCountForConversationAsync(int conversationId, string userId);
    }
}
