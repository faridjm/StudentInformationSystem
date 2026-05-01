using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Enums;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AnnouncementRepository : GenericRepository<Announcement>, IAnnouncementRepository
    {
        public AnnouncementRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Announcement>> GetGlobalAnnouncementsAsync()
            => await _dbSet
                .Include(a => a.Author)
                .Where(a => a.TargetType == AnnouncementTarget.All
                         || a.TargetType == AnnouncementTarget.Students)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Announcement>> GetCourseAnnouncementsAsync(int courseOfferingId)
            => await _dbSet
                .Include(a => a.Author)
                .Where(a => a.CourseOfferingId == courseOfferingId
                         && a.TargetType == AnnouncementTarget.Course)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Announcement>> GetAnnouncementsForStudentAsync(int studentId)
            => await _dbSet
                .Include(a => a.Author)
                .Include(a => a.CourseOffering)
                .Where(a => a.TargetType == AnnouncementTarget.All
                         || a.TargetType == AnnouncementTarget.Students
                         || (a.TargetType == AnnouncementTarget.Course
                             && a.CourseOffering!.Enrollments
                                 .Any(e => e.StudentId == studentId)))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
    }
}

