using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Bir konkret dərs günü. Həftəlik şablondan generasiya olunur,
    /// amma müəllim sonradan ayrı-ayrılıqda redaktə edə bilər.
    /// </summary>
    public class ClassSession : BaseEntity
    {
        public int CourseOfferingId { get; set; }
        public CourseOffering CourseOffering { get; set; } = null!;

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Room { get; set; }

        public ClassSessionStatus Status { get; set; } = ClassSessionStatus.Scheduled;

        /// <summary>Sessiyanın mövzusu (məs. "Calculus II — Limits"). Müəllim yazır.</summary>
        public string? Topic { get; set; }

        /// <summary>Müəllimin əlavə qeydləri (material linkləri, mənbələr).</summary>
        public string? Notes { get; set; }

        /// <summary>Ləğv olunma səbəbi (Status=Cancelled olarsa məcburi).</summary>
        public string? CancellationReason { get; set; }

        /// <summary>Əvəzedici sessiyalar üçün hansı sessiyanın əvəzi olduğunu göstərir.</summary>
        public int? OriginalSessionId { get; set; }
        public ClassSession? OriginalSession { get; set; }

        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
