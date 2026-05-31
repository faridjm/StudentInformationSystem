using Refit;
using SIS.BLL.Services.DTOs.Assignment;

namespace SIS.UI.ApiClients
{
    public interface IAssignmentApi
    {
        [Get("/api/assignment/group/{groupId}")]
        Task<IEnumerable<AssignmentDto>> GetByGroupAsync(int groupId);

        [Get("/api/assignment/{id}")]
        Task<AssignmentDto> GetByIdAsync(int id);

        [Post("/api/assignment")]
        Task<AssignmentDto> CreateAsync([Body] CreateAssignmentDto dto);

        [Put("/api/assignment")]
        Task UpdateAsync([Body] UpdateAssignmentDto dto);

        [Delete("/api/assignment/{id}")]
        Task DeleteAsync(int id);

        [Get("/api/assignment/{id}/submissions")]
        Task<IEnumerable<SubmissionDto>> GetSubmissionsAsync(int id);

        [Get("/api/assignment/my-submissions/{enrollmentId}")]
        Task<IEnumerable<SubmissionDto>> GetMySubmissionsAsync(int enrollmentId);

        [Post("/api/assignment/submit")]
        Task<SubmissionDto> SubmitAsync([Body] CreateSubmissionDto dto);

        [Post("/api/assignment/grade")]
        Task GradeAsync([Body] GradeSubmissionDto dto);
    }
}
