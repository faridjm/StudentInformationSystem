using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AssessmentSubmissionRepository : GenericRepository<AssessmentSubmission>, IAssessmentSubmissionRepository
    {
        public AssessmentSubmissionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<AssessmentSubmission>> GetByAssessmentItemAsync(int itemId)
            => await _dbSet
                .Include(s => s.Enrollment).ThenInclude(e => e.Student).ThenInclude(st => st.AppUser)
                .Where(s => s.AssessmentItemId == itemId)
                .ToListAsync();

        public async Task<IEnumerable<AssessmentSubmission>> GetByEnrollmentAsync(int enrollmentId)
            => await _dbSet
                .Include(s => s.AssessmentItem)
                .Where(s => s.EnrollmentId == enrollmentId)
                .OrderBy(s => s.AssessmentItem.Date)
                .ToListAsync();

        public async Task<AssessmentSubmission?> GetByItemAndEnrollmentAsync(int itemId, int enrollmentId)
            => await _dbSet
                .FirstOrDefaultAsync(s => s.AssessmentItemId == itemId && s.EnrollmentId == enrollmentId);
    }
}
