using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAnnouncementRecipientRepository : IGenericRepository<AnnouncementRecipient>
    {
        Task<AnnouncementRecipient?> GetAsync(int announcementId, string userId);
        Task<IEnumerable<AnnouncementRecipient>> GetByAnnouncementAsync(int announcementId);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
