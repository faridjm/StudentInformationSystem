using SIS.Entities.Enums;

namespace SIS.UI.ViewModels
{
    /// <summary>
    /// Admin Student Detail səhifəsində "Academic Information" bloku üçün
    /// hər enrollment-in akademik xülasəsi. Data Enrollment + Transcript
    /// servislərindən gəlir; heç bir schema dəyişikliyi yoxdur.
    /// </summary>
    public class StudentAcademicRowVm
    {
        public int EnrollmentId { get; set; }
        public string CourseName { get; set; } = "—";
        public string GroupName { get; set; } = "—";
        public string? AcademicYear { get; set; }
        public string TeacherName { get; set; } = "—";
        public EnrollmentStatus Status { get; set; }
        public DateTime EnrolledAt { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public double? AttendancePercentage { get; set; }
        public decimal? OfficialGrade { get; set; }
        public decimal? GPA { get; set; }
        public bool? CertificateEligible { get; set; }

        /// <summary>Transcript xülasəsi uğurla yükləndimi.</summary>
        public bool TranscriptLoaded { get; set; }
    }
}
