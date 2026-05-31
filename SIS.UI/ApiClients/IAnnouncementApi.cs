using Refit;
using SIS.BLL.Services.DTOs.Announcement;

namespace SIS.UI.ApiClients
{
    public interface IAnnouncementApi
    {
        [Get("/api/announcement/global")]
        Task<IEnumerable<AnnouncementDto>> GetGlobalAsync();

        [Get("/api/announcement/course/{courseOfferingId}")]
        Task<IEnumerable<AnnouncementDto>> GetByCourseAsync(int courseOfferingId);

        [Get("/api/announcement/group/{groupId}")]
        Task<IEnumerable<AnnouncementDto>> GetByGroupAsync(int groupId);

        [Get("/api/announcement/my-announcements")]
        Task<IEnumerable<AnnouncementDto>> GetMyAnnouncementsAsync();

        [Get("/api/announcement/my-teacher-announcements")]
        Task<IEnumerable<AnnouncementDto>> GetMyTeacherAnnouncementsAsync();

        [Post("/api/announcement")]
        Task CreateAsync([Body] CreateAnnouncementDto dto);

        [Delete("/api/announcement/{id}")]
        Task DeleteAsync(int id);
    }


}
