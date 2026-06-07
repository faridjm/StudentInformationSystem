using Refit;
using SIS.BLL.Services.DTOs.Announcement;

namespace SIS.UI.ApiClients
{
    public interface IAnnouncementApi
    {
        [Get("/api/announcement")]
        Task<IEnumerable<AnnouncementDto>> GetAllAsync();

        [Get("/api/announcement/global")]
        Task<IEnumerable<AnnouncementDto>> GetGlobalAsync();

        [Get("/api/announcement/group/{groupId}")]
        Task<IEnumerable<AnnouncementDto>> GetByGroupAsync(int groupId);

        [Get("/api/announcement/{id}")]
        Task<AnnouncementDetailDto> GetDetailAsync(int id);

        [Get("/api/announcement/my")]
        Task<IEnumerable<AnnouncementDto>> GetMyAsync();

        [Get("/api/announcement/my-announcements")]
        Task<IEnumerable<AnnouncementDto>> GetMyAnnouncementsAsync();

        [Get("/api/announcement/my-teacher-announcements")]
        Task<IEnumerable<AnnouncementDto>> GetMyTeacherAnnouncementsAsync();

        [Post("/api/announcement/{id}/read")]
        Task MarkAsReadAsync(int id);

        [Post("/api/announcement")]
        Task CreateAsync([Body] CreateAnnouncementDto dto);

        [Post("/api/announcement/group")]
        Task CreateGroupAsync([Body] TeacherGroupAnnouncementDto dto);

        [Delete("/api/announcement/{id}")]
        Task DeleteAsync(int id);

        [Get("/api/announcement/search-users")]
        Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync([AliasAs("query")] string query);
    }
}
