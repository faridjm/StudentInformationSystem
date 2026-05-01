using SIS.BLL.Services.DTOs.Enrollment;

namespace SIS.BLL.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<IEnumerable<EnrollmentListDto>> GetAllEnrollmentsAsync();
        Task<EnrollmentDetailDto?> GetEnrollmentByIdAsync(int id);
        Task<IEnumerable<EnrollmentListDto>> GetStudentEnrollmentsAsync(int studentId);
        Task<IEnumerable<EnrollmentListDto>> GetCourseEnrollmentsAsync(int courseOfferingId);
        Task EnrollStudentAsync(CreateEnrollmentDto dto);
        Task DropEnrollmentAsync(int enrollmentId);
    }
}
