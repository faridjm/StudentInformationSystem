using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAnnouncementRepository : IGenericRepository<Announcement>
    {
        Task<IEnumerable<Announcement>> GetGlobalAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetCourseAnnouncementsAsync(int courseOfferingId);
        Task<IEnumerable<Announcement>> GetAnnouncementsForStudentAsync(int studentId);
    }
}
