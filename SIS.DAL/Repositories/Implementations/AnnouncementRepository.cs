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

        public async Task<IEnumerable<Announcement>> GetGlobalAnnouncementsAsync()
            => await _dbSet
                .Include(a => a.Author)
                .Where(a => a.TargetType == AnnouncementTarget.All
                         || a.TargetType == AnnouncementTarget.Teachers)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Announcement>> GetForStudentAsync(int studentId)
            => await _dbSet
                .Include(a => a.Author)
                .Where(a => a.TargetType == AnnouncementTarget.All
                         || a.TargetType == AnnouncementTarget.Students)
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
