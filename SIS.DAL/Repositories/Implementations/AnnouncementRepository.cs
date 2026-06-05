using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Enums;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AnnouncementRepository : GenericRepository<Announcement>, IAnnouncementRepository
    {
        public AnnouncementRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Announcement>> GetAllWithAuthorsAsync()
            => await _dbSet
                .Include(a => a.Author)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Announcement>> GetAllWithRecipientsAsync()
            => await _dbSet
                .Include(a => a.Author)
                .Include(a => a.Group)
                .Include(a => a.Recipients).ThenInclude(r => r.User)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        public async Task<Announcement?> GetWithRecipientsAsync(int announcementId)
            => await _dbSet
                .Include(a => a.Author)
                .Include(a => a.Group)
                .Include(a => a.Recipients).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(a => a.Id == announcementId);

        public async Task<IEnumerable<Announcement>> GetForUserAsync(string userId)
            => await _dbSet
                .Include(a => a.Author)
                .Include(a => a.Recipients)
                .Where(a => a.Recipients.Any(r => r.UserId == userId && !r.IsDeleted))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        // Geriyə uyğunluq — bu metod artıq recipient əsasında işləyir (Student-in AppUserId-i ilə).
        public async Task<IEnumerable<Announcement>> GetForStudentAsync(int studentId)
        {
            var student = await _context.Set<Student>().FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null) return Enumerable.Empty<Announcement>();
            return await GetForUserAsync(student.AppUserId);
        }

        public async Task<IEnumerable<Announcement>> GetGlobalAnnouncementsAsync()
            => await _dbSet
                .Include(a => a.Author)
                .Where(a => a.TargetType == AnnouncementTarget.All
                         || a.TargetType == AnnouncementTarget.Teachers)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Announcement>> GetByGroupAsync(int groupId)
            => await _dbSet
                .Include(a => a.Author)
                .Where(a => a.GroupId == groupId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
    }
}
