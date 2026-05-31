using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAssessmentSubmissionRepository : IGenericRepository<AssessmentSubmission>
    {
        Task<IEnumerable<AssessmentSubmission>> GetByAssessmentItemAsync(int itemId);
        Task<IEnumerable<AssessmentSubmission>> GetByEnrollmentAsync(int enrollmentId);
        Task<AssessmentSubmission?> GetByItemAndEnrollmentAsync(int itemId, int enrollmentId);
    }
}
