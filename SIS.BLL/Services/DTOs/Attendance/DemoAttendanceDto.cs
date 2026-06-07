namespace SIS.BLL.Services.DTOs.Attendance
{
    /// <summary>Demo (təqdimat) attendance generasiya rejimləri.</summary>
    public enum DemoAttendanceMode
    {
        /// <summary>Bütün tələbələr Present olaraq qeyd olunur.</summary>
        AllPresent = 0,

        /// <summary>Tələbələrə fərqli profillər (yüksək/orta/risk) verir, Late/Excused qarışıqdır.</summary>
        RealisticDemo = 1,

        /// <summary>Müəllim status saylarını özü təyin edir (Present/Absent/Late/Excused).</summary>
        CustomCounts = 2
    }

    /// <summary>Demo attendance generasiya sorğusu (Teacher panel).</summary>
    public class GenerateDemoAttendanceDto
    {
        public int GroupId { get; set; }
        public int FromSessionNumber { get; set; }
        public int ToSessionNumber { get; set; }
        public DemoAttendanceMode Mode { get; set; } = DemoAttendanceMode.RealisticDemo;
        public bool OverwriteExisting { get; set; }

        // CustomCounts üçün — cəmi seçilmiş sessiya sayına bərabər olmalıdır
        public int? PresentCount { get; set; }
        public int? AbsentCount { get; set; }
        public int? LateCount { get; set; }
        public int? ExcusedCount { get; set; }
    }

    /// <summary>Demo generation nəticəsi.</summary>
    public class DemoAttendanceResultDto
    {
        public int CreatedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int SkippedCount { get; set; }
        public int SessionsAffected { get; set; }
        public int StudentsAffected { get; set; }
        public string Message { get; set; } = "";
    }
}
