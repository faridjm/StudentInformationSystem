using SIS.BLL.Services.DTOs.Attendance;

namespace SIS.BLL.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<IEnumerable<AttendanceDto>> GetAttendanceByEnrollmentAsync(int enrollmentId);
        Task<IEnumerable<AttendanceDto>> GetAttendanceByDateAsync(int courseOfferingId, DateTime date);
        Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int enrollmentId);
        Task RecordAttendanceAsync(CreateAttendanceDto dto);
        Task RecordBulkAttendanceAsync(IEnumerable<CreateAttendanceDto> dtos);
    }
}
