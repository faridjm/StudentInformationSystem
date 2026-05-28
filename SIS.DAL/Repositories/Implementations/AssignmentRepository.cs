using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AssignmentRepository : GenericRepository<Assignment>, IAssignmentRepository
    {
        public AssignmentRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Assignment>> GetByCourseOfferingAsync(int courseOfferingId)
            => await _dbSet
                .Where(a => a.CourseOfferingId == courseOfferingId)
                .OrderByDescending(a => a.DueDate)
                .ToListAsync();

        public async Task<Assignment?> GetWithSubmissionsAsync(int assignmentId)
            => await _dbSet
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Enrollment)
                        .ThenInclude(e => e.Student)
                            .ThenInclude(st => st.AppUser)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);
    }
}
