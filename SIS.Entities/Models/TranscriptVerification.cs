using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Transcript PDF-lər üçün QR-əsaslı doğrulama qeydi.
    /// Public /VerifyTranscript/{code} səhifəsi bu qeyddən istifadə edir.
    /// Qeyd: bu entity calculation logic-ə toxunmur — yalnız PDF generasiya
    /// anındakı dəyərlərin "snapshot"-unu saxlayır ki, doğrulama zamanı göstərilsin.
    /// StudentId / GroupId burada sadəcə int olaraq saxlanılır (FK navigation yoxdur).
    /// </summary>
    public class TranscriptVerification : BaseEntity
    {
        /// <summary>URL-safe, təxmin edilməsi çətin doğrulama kodu.</summary>
        public string VerificationCode { get; set; } = null!;

        public int StudentId { get; set; }
        public int GroupId { get; set; }
        public int EnrollmentId { get; set; }

        public TranscriptType TranscriptType { get; set; }

        /// <summary>Qeydin yaradıldığı dil (az|en|ru) — informativ.</summary>
        public string Language { get; set; } = "az";

        // PDF generasiya anındakı snapshot dəyərləri (calculation-dan gəlir, dəyişdirilmir)
        public decimal OfficialGrade { get; set; }
        public decimal GPA { get; set; }
        public double AttendancePercentage { get; set; }

        /// <summary>Kanonik kod: Pending / NotCompleted / NormalCompletion / HonorCompletion / HighHonorCompletion / NotEligible.</summary>
        public string CompletionLevel { get; set; } = "Pending";

        /// <summary>Kanonik kod: Pending / Eligible / NotEligible.</summary>
        public string CertificateStatus { get; set; } = "Pending";

        public DateTime IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
