using SIS.BLL.Services.DTOs.Assessment;

namespace SIS.BLL.Services.Interfaces
{
    public interface IAssessmentService
    {
        Task<IEnumerable<AssessmentItemDto>> GetByGroupAsync(int groupId);
        Task<AssessmentItemDto?> GetByIdAsync(int id);
        Task<AssessmentItemDto> CreateAsync(CreateAssessmentItemDto dto);
        Task UpdateAsync(UpdateAssessmentItemDto dto);
        Task DeleteAsync(int id);

        // Submissions
        Task<IEnumerable<AssessmentSubmissionDto>> GetSubmissionsAsync(int assessmentItemId);
        Task<IEnumerable<AssessmentSubmissionDto>> GetMySubmissionsAsync(int enrollmentId);
        Task GradeAsync(GradeAssessmentDto dto);
    }
}
