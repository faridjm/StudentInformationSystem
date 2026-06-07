using SIS.BLL.Services.DTOs.Attendance;

namespace SIS.BLL.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<IEnumerable<AttendanceDto>> GetAttendanceByEnrollmentAsync(int enrollmentId);
        Task<IEnumerable<AttendanceDto>> GetAttendanceBySessionAsync(int sessionId);
        Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int enrollmentId);
        Task RecordBulkAttendanceAsync(IEnumerable<CreateAttendanceDto> dtos);

        /// <summary>
        /// Yalnız təqdimat/test üçün demo davamiyyət generasiyası.
        /// Yalnız qrupun sahibi olan müəllim çağıra bilər.
        /// </summary>
        Task<DemoAttendanceResultDto> GenerateTeacherDemoAttendanceAsync(
            GenerateDemoAttendanceDto dto, string teacherUserId);
    }
}
