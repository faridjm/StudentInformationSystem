using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAssignmentSubmissionRepository : IGenericRepository<AssignmentSubmission>
    {
        Task<IEnumerable<AssignmentSubmission>> GetByAssignmentAsync(int assignmentId);
        Task<IEnumerable<AssignmentSubmission>> GetByEnrollmentAsync(int enrollmentId);
        Task<AssignmentSubmission?> GetByAssignmentAndEnrollmentAsync(int assignmentId, int enrollmentId);
    }
}
