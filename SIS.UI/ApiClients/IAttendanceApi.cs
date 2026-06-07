using Refit;
using SIS.BLL.Services.DTOs.Attendance;

namespace SIS.UI.ApiClients
{
    public interface IAttendanceApi
    {
        [Get("/api/attendance/enrollment/{enrollmentId}")]
        Task<IEnumerable<AttendanceDto>> GetByEnrollmentAsync(int enrollmentId);

        [Get("/api/attendance/enrollment/{enrollmentId}/summary")]
        Task<AttendanceSummaryDto> GetSummaryAsync(int enrollmentId);

        [Get("/api/attendance/course/{courseOfferingId}/date/{date}")]
        Task<IEnumerable<AttendanceDto>> GetByDateAsync(int courseOfferingId, DateTime date);

        [Post("/api/attendance")]
        Task RecordAsync([Body] CreateAttendanceDto dto);

        [Post("/api/attendance/bulk")]
        Task RecordBulkAsync([Body] IEnumerable<CreateAttendanceDto> dtos);

        [Post("/api/attendance/demo")]
        Task<DemoAttendanceResultDto> GenerateDemoAsync([Body] GenerateDemoAttendanceDto dto);

        [Get("/api/attendance/session/{sessionId}")]
        Task<IEnumerable<AttendanceDto>> GetBySessionAsync(int sessionId);
    }


}
