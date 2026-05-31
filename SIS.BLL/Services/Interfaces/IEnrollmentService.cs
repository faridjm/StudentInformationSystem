using SIS.BLL.Services.DTOs.Enrollment;

namespace SIS.BLL.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<IEnumerable<EnrollmentListDto>> GetByGroupAsync(int groupId);
        Task<IEnumerable<EnrollmentListDto>> GetByStudentAsync(int studentId);
        Task<EnrollmentListDto?> GetByIdAsync(int enrollmentId);
        Task EnrollAsync(CreateEnrollmentDto dto);
        Task DropAsync(int enrollmentId);
    }
}
