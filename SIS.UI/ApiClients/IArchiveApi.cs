using Refit;
using SIS.BLL.Services.DTOs.Archive;
using SIS.BLL.Services.DTOs.Common;

namespace SIS.UI.ApiClients
{
    public interface IArchiveApi
    {
        [Get("/api/archive/chats")]
        Task<PagedResult<ArchiveConversationListItemDto>> GetChatsAsync(
            string? search, string? role, string? status,
            DateTime? fromDate, DateTime? toDate, int? groupId,
            int page, int pageSize);

        [Get("/api/archive/chats/{conversationId}")]
        Task<ArchiveConversationDetailDto> GetDetailAsync(int conversationId);

        [Post("/api/archive/chats/{conversationId}/archive")]
        Task ArchiveAsync(int conversationId);

        [Post("/api/archive/chats/{conversationId}/restore")]
        Task RestoreAsync(int conversationId);
    }
}
