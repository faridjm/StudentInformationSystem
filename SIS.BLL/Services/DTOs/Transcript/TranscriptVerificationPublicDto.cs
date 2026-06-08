using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Transcript
{
    /// <summary>
    /// Public /VerifyTranscript/{code} səhifəsi üçün məlumat.
    /// Məxfilik: email, telefon, tam assessment breakdown, detallı davamiyyət,
    /// daxili ID-lər BURADA göstərilmir.
    /// </summary>
    public class TranscriptVerificationPublicDto
    {
        /// <summary>Valid | Invalid | Revoked | Expired</summary>
        public string Status { get; set; } = "Invalid";

        public string? VerificationCode { get; set; }

        // Yalnız Status = Valid olduqda doldurulur
        public string? StudentFullName { get; set; }
        public string? CourseName { get; set; }
        public string? GroupName { get; set; }

        public TranscriptType? TranscriptType { get; set; }
        public decimal? OfficialGrade { get; set; }
        public decimal? GPA { get; set; }
        public double? AttendancePercentage { get; set; }
        public string? CompletionLevel { get; set; }
        public string? CertificateStatus { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
