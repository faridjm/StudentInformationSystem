using Refit;
using SIS.BLL.Services.DTOs.Parent;

namespace SIS.UI.ApiClients
{
    public interface IStudentParentApi
    {
        [Get("/api/students/{studentId}/parents")]
        Task<IEnumerable<ParentInfoDto>> GetParentsAsync(int studentId);

        [Post("/api/students/{studentId}/parents")]
        Task<ParentCreatedResultDto> CreateAsync(int studentId, [Body] CreateParentForStudentDto dto);

        [Post("/api/students/{studentId}/parents/link")]
        Task LinkAsync(int studentId, [Body] LinkExistingParentDto dto);

        [Patch("/api/students/{studentId}/parents/{parentUserId}/disable")]
        Task DisableAsync(int studentId, string parentUserId);

        [Patch("/api/students/{studentId}/parents/{parentUserId}/enable")]
        Task EnableAsync(int studentId, string parentUserId);
    }
}
