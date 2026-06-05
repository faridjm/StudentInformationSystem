using SIS.BLL.Services.DTOs.Archive;
using SIS.BLL.Services.DTOs.Common;

namespace SIS.BLL.Services.Interfaces
{
    /// <summary>
    /// Arxiv mərkəzi (yalnız SuperAdmin). Hazırda Chat conversation arxivinə fokuslanıb,
    /// gələcəkdə deleted/inactive entity arxivləri də buraya əlavə oluna bilər.
    /// </summary>
    public interface IArchiveService
    {
        Task<PagedResult<ArchiveConversationListItemDto>> GetChatConversationsAsync(ArchiveChatFilterDto filter);
        Task<ArchiveConversationDetailDto?> GetChatConversationDetailAsync(int conversationId);
        Task ArchiveConversationAsync(int conversationId, string superAdminId);
        Task RestoreConversationAsync(int conversationId, string superAdminId);
    }
}
