using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AnnouncementRecipientRepository : GenericRepository<AnnouncementRecipient>, IAnnouncementRecipientRepository
    {
        public AnnouncementRecipientRepository(AppDbContext context) : base(context) { }

        public async Task<AnnouncementRecipient?> GetAsync(int announcementId, string userId)
            => await _dbSet
                .Include(r => r.Announcement)
                .FirstOrDefaultAsync(r => r.AnnouncementId == announcementId && r.UserId == userId);

        public async Task<IEnumerable<AnnouncementRecipient>> GetByAnnouncementAsync(int announcementId)
            => await _dbSet
                .Include(r => r.User)
                .Where(r => r.AnnouncementId == announcementId)
                .ToListAsync();

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _dbSet.CountAsync(r => r.UserId == userId && !r.IsRead);
    }
}
