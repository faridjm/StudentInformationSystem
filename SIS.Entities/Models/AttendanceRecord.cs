using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    public class AttendanceRecord : BaseEntity
    {
        public DateTime SessionDate { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;

        /// <summary>
        /// Hansı konkret dərs sessiyasına aiddir. Köhnə qeydlərdə null ola bilər
        /// (Phase 2E-dən əvvəl yaradılanlar). Yeni qeydlərdə doldurulur.
        /// </summary>
        public int? ClassSessionId { get; set; }
        public ClassSession? ClassSession { get; set; }
    }
}
