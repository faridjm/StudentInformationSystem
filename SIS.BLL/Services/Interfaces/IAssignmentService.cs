using SIS.BLL.Services.DTOs.Assignment;

namespace SIS.BLL.Services.Interfaces
{
    public interface IAssignmentService
    {
        Task<IEnumerable<AssignmentDto>> GetByCourseOfferingAsync(int courseOfferingId);
        Task<AssignmentDto?> GetByIdAsync(int id);
        Task<AssignmentDto> CreateAsync(CreateAssignmentDto dto);
        Task UpdateAsync(UpdateAssignmentDto dto);
        Task DeleteAsync(int id);

        // Submissions
        Task<IEnumerable<SubmissionDto>> GetSubmissionsAsync(int assignmentId);
        Task<IEnumerable<SubmissionDto>> GetMySubmissionsAsync(int enrollmentId);
        Task<SubmissionDto?> GetSubmissionAsync(int assignmentId, int enrollmentId);
        Task<SubmissionDto> SubmitAsync(CreateSubmissionDto dto);
        Task GradeAsync(GradeSubmissionDto dto);
    }
}
