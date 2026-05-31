using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAnnouncementRepository : IGenericRepository<Announcement>
    {
        Task<IEnumerable<Announcement>> GetAllWithAuthorsAsync();
        Task<IEnumerable<Announcement>> GetForStudentAsync(int studentId);
        Task<IEnumerable<Announcement>> GetGlobalAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetByGroupAsync(int groupId);
    }
}
