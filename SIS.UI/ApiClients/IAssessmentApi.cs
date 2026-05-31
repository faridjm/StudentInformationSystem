using Refit;
using SIS.BLL.Services.DTOs.Assessment;

namespace SIS.UI.ApiClients
{
    public interface IAssessmentApi
    {
        [Get("/api/assessment/group/{groupId}")]
        Task<IEnumerable<AssessmentItemDto>> GetByGroupAsync(int groupId);

        [Get("/api/assessment/{id}")]
        Task<AssessmentItemDto> GetByIdAsync(int id);

        [Post("/api/assessment")]
        Task<AssessmentItemDto> CreateAsync([Body] CreateAssessmentItemDto dto);

        [Put("/api/assessment")]
        Task UpdateAsync([Body] UpdateAssessmentItemDto dto);

        [Delete("/api/assessment/{id}")]
        Task DeleteAsync(int id);

        [Get("/api/assessment/{id}/submissions")]
        Task<IEnumerable<AssessmentSubmissionDto>> GetSubmissionsAsync(int id);

        [Get("/api/assessment/my-submissions/{enrollmentId}")]
        Task<IEnumerable<AssessmentSubmissionDto>> GetMySubmissionsAsync(int enrollmentId);

        [Post("/api/assessment/grade")]
        Task GradeAsync([Body] GradeAssessmentDto dto);
    }
}
