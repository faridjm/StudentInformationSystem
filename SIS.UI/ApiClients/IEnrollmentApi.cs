using Refit;
using SIS.BLL.Services.DTOs.Enrollment;

namespace SIS.UI.ApiClients
{
    public interface IEnrollmentApi
    {
        [Get("/api/enrollment/group/{groupId}")]
        Task<IEnumerable<EnrollmentListDto>> GetByGroupAsync(int groupId);

        [Get("/api/enrollment/student/{studentId}")]
        Task<IEnumerable<EnrollmentListDto>> GetByStudentAsync(int studentId);

        [Get("/api/enrollment/my")]
        Task<IEnumerable<EnrollmentListDto>> GetMyEnrollmentsAsync();

        [Post("/api/enrollment")]
        Task EnrollAsync([Body] CreateEnrollmentDto dto);

        [Delete("/api/enrollment/{id}")]
        Task DropAsync(int id);
    }
}
