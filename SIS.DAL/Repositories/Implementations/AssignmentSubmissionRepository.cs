using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AssignmentSubmissionRepository : GenericRepository<AssignmentSubmission>, IAssignmentSubmissionRepository
    {
        public AssignmentSubmissionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<AssignmentSubmission>> GetByAssignmentAsync(int assignmentId)
            => await _dbSet
                .Include(s => s.Enrollment).ThenInclude(e => e.Student).ThenInclude(st => st.AppUser)
                .Where(s => s.AssignmentId == assignmentId)
                .OrderBy(s => s.SubmittedAt)
                .ToListAsync();

        public async Task<IEnumerable<AssignmentSubmission>> GetByEnrollmentAsync(int enrollmentId)
            => await _dbSet
                .Include(s => s.Assignment)
                .Where(s => s.EnrollmentId == enrollmentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

        public async Task<AssignmentSubmission?> GetByAssignmentAndEnrollmentAsync(int assignmentId, int enrollmentId)
            => await _dbSet
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.EnrollmentId == enrollmentId);
    }
}
