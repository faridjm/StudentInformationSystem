using SIS.BLL.Services.DTOs.Announcement;

namespace SIS.BLL.Services.Interfaces
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<AnnouncementDto>> GetAllAsync();

        /// <summary>Köhnə qlobal görüş — geriyə uyğunluq üçün.</summary>
        Task<IEnumerable<AnnouncementDto>> GetGlobalAnnouncementsAsync();

        Task<IEnumerable<AnnouncementDto>> GetForStudentAsync(int studentId);
        Task<IEnumerable<AnnouncementDto>> GetAnnouncementsForStudentAsync(int studentId);

        /// <summary>Konkret istifadəçi (Student/Teacher) öz aldığı elanları görür.</summary>
        Task<IEnumerable<AnnouncementDto>> GetForUserAsync(string userId);

        Task<AnnouncementDetailDto?> GetDetailAsync(int id, string? viewerUserId = null);

        /// <summary>İstifadəçi elanı oxudu — recipient.IsRead = true.</summary>
        Task MarkAsReadAsync(int announcementId, string userId);

        Task CreateAnnouncementAsync(CreateAnnouncementDto dto, string authorId);
        Task DeleteAnnouncementAsync(int id);
        Task<IEnumerable<AnnouncementDto>> GetByGroupAsync(int groupId);

        /// <summary>SelectedUsers üçün axtarış: ad/soyad/email/role üzrə.</summary>
        Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(string query);
    }
}
