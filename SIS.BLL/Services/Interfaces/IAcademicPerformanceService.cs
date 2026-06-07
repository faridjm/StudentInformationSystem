using SIS.BLL.Services.DTOs.Attendance;
using SIS.BLL.Services.DTOs.Performance;

namespace SIS.BLL.Services.Interfaces
{
    /// <summary>
    /// Mərkəzləşdirilmiş akademik göstərici hesablamaları.
    /// Attendance %, Official Grade, GPA, Homework Average və Certificate
    /// Eligibility — hamısı bu service vasitəsilə hesablanır ki, panel və PDF
    /// eyni nəticəni göstərsin.
    /// </summary>
    public interface IAcademicPerformanceService
    {
        /// <summary>Bir tələbənin bir qrup üzrə tam performans paketi.</summary>
        Task<AcademicPerformanceDto> GetEnrollmentPerformanceAsync(int enrollmentId);

        /// <summary>Davamiyyət xülasəsi (mövcud AttendanceSummaryDto formasında).</summary>
        Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(int enrollmentId);

        /// <summary>0–100 faizdən GPA (0.0–4.0) çıxarır.</summary>
        decimal CalculateGpa(decimal officialGradePercent);
    }
}
