using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class CourseOfferingRepository : GenericRepository<CourseOffering>, ICourseOfferingRepository
    {
        public CourseOfferingRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<CourseOffering?> GetOfferingWithDetailsAsync(int offeringId)
            => await _dbSet
                .Include(co => co.Course)
                .Include(co => co.AcademicTerm)
                .Include(co => co.Teacher)
                    .ThenInclude(t => t.AppUser)
                .Include(co => co.Enrollments)
                .FirstOrDefaultAsync(co => co.Id == offeringId);

        public async Task<IEnumerable<CourseOffering>> GetActiveOfferingsAsync()
            => await _dbSet
                .Include(co => co.Course)
                .Include(co => co.Teacher)
                    .ThenInclude(t => t.AppUser)
                .Where(co => co.IsActive)
                .ToListAsync();

        public async Task<IEnumerable<CourseOffering>> GetOfferingsByTeacherAsync(int teacherId)
            => await _dbSet
                .Include(co => co.Course)
                .Include(co => co.AcademicTerm)
                .Where(co => co.TeacherId == teacherId)
                .ToListAsync();

        public async Task<IEnumerable<CourseOffering>> GetOfferingsByTermAsync(int termId)
            => await _dbSet
                .Include(co => co.Course)
                .Include(co => co.Teacher)
                    .ThenInclude(t => t.AppUser)
                .Where(co => co.AcademicTermId == termId)
                .ToListAsync();

        public async Task<bool> HasCapacityAsync(int offeringId)
        {
            var offering = await _dbSet
                .Include(co => co.Enrollments)
                .FirstOrDefaultAsync(co => co.Id == offeringId);

            if (offering == null) return false;

            return offering.Enrollments.Count < offering.Capacity;
        }
    }

}

