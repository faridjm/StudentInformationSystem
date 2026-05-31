using SIS.BLL.Services.DTOs.Announcement;

namespace SIS.BLL.Services.Interfaces
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<AnnouncementDto>> GetAllAsync();
        Task<IEnumerable<AnnouncementDto>> GetGlobalAnnouncementsAsync();
        Task<IEnumerable<AnnouncementDto>> GetForStudentAsync(int studentId);
        Task<IEnumerable<AnnouncementDto>> GetAnnouncementsForStudentAsync(int studentId);
        Task CreateAnnouncementAsync(CreateAnnouncementDto dto, string authorId);
        Task DeleteAnnouncementAsync(int id);
        Task<IEnumerable<AnnouncementDto>> GetByGroupAsync(int groupId);
    }
}
