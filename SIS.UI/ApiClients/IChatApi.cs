using Refit;
using SIS.BLL.Services.DTOs.Chat;

namespace SIS.UI.ApiClients
{
    public interface IChatApi
    {
        [Get("/api/chat/conversations")]
        Task<IEnumerable<ConversationDto>> GetConversationsAsync();

        [Post("/api/chat/conversations")]
        Task<ConversationDto> StartConversationAsync([Body] StartConversationDto dto);

        [Get("/api/chat/conversations/{id}/messages")]
        Task<IEnumerable<MessageDto>> GetMessagesAsync(int id, int take = 50, int skip = 0);

        [Post("/api/chat/conversations/{id}/messages")]
        Task<MessageDto> SendMessageAsync(int id, [Body] SendMessageDto dto);

        /// <summary>JSON metadata — fayl UI tərəfində artıq saxlanılıb.</summary>
        [Post("/api/chat/conversations/{id}/messages/attachment-meta")]
        Task<MessageDto> SendAttachmentMetaAsync(int id, [Body] SendAttachmentMessageDto dto);

        [Put("/api/chat/conversations/{id}/read")]
        Task MarkAsReadAsync(int id);

        [Get("/api/chat/unread-count")]
        Task<UnreadCountResponse> GetUnreadCountAsync();
    }

    public class UnreadCountResponse { public int Count { get; set; } }
}
