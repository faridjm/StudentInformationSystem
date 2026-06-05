using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IArchiveRepository
    {
        Task<(IEnumerable<ChatConversation> Items, int Total)> GetChatConversationsAsync(
            string? search, string? role, string? status,
            DateTime? fromDate, DateTime? toDate, int? groupId,
            int page, int pageSize);

        Task<ChatConversation?> GetChatConversationDetailAsync(int conversationId);

        Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId);

        Task<int> GetMessageCountAsync(int conversationId);

        /// <summary>Conversation-ın son mesajını qaytarır (preview üçün).</summary>
        Task<Dictionary<int, ChatMessage>> GetLastMessagesAsync(IEnumerable<int> conversationIds);

        /// <summary>Hər conversation üçün cəmi mesaj sayı.</summary>
        Task<Dictionary<int, int>> GetMessageCountsAsync(IEnumerable<int> conversationIds);
    }
}
