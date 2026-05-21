using Refit;
using SIS.BLL.Services.DTOs.Enrollment;

namespace SIS.UI.ApiClients
{
    public interface IEnrollmentApi
    {
        [Get("/api/enrollment")]
        Task<IEnumerable<EnrollmentListDto>> GetAllAsync();

        [Get("/api/enrollment/{id}")]
        Task<EnrollmentDetailDto> GetByIdAsync(int id);

        [Get("/api/enrollment/student/{studentId}")]
        Task<IEnumerable<EnrollmentListDto>> GetByStudentAsync(int studentId);

        [Get("/api/enrollment/my-enrollments")]
        Task<IEnumerable<EnrollmentListDto>> GetMyEnrollmentsAsync();

        [Get("/api/enrollment/course/{courseOfferingId}")]
        Task<IEnumerable<EnrollmentListDto>> GetByCourseAsync(int courseOfferingId);

        [Post("/api/enrollment")]
        Task EnrollAsync([Body] CreateEnrollmentDto dto);

        [Put("/api/enrollment/{id}/drop")]
        Task DropAsync(int id);
    }


}
