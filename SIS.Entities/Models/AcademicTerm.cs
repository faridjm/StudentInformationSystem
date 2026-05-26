using SIS.Entities.Base;
using SIS.Entities.Enums;

namespace SIS.Entities.Models
{
    /// <summary>
    /// Semestr. Bir AcademicYear-in altında 4-ə qədər ola bilər.
    /// </summary>
    public class AcademicTerm : BaseEntity
    {
        public string Name { get; set; } = null!;

        /// <summary>Dərslərin başladığı gün (köhnə kodda "StartDate" idi).</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Dərslərin bitdiyi gün (köhnə kodda "EndDate" idi).</summary>
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = false;

        // Yeni tarix sahələri — köhnə məlumatlar üçün nullable.
        // Yeni Term yaradılarkən bunların doldurulması tələb olunur.

        /// <summary>Tələbə qeydiyyatı açılır.</summary>
        public DateTime? RegistrationStart { get; set; }

        /// <summary>Tələbə qeydiyyatı bağlanır.</summary>
        public DateTime? RegistrationEnd { get; set; }

        /// <summary>Bu tarixdən sonra kursdan çıxma cərimə (F) ilə olur.</summary>
        public DateTime? DropDeadline { get; set; }

        /// <summary>İmtahan dövrü başlayır.</summary>
        public DateTime? ExamPeriodStart { get; set; }

        /// <summary>İmtahan dövrü bitir.</summary>
        public DateTime? ExamPeriodEnd { get; set; }

        /// <summary>Müəllimin qiymət düzəlişi son tarixi. Sonra hər şey kilidlənir.</summary>
        public DateTime? GradeSubmissionDeadline { get; set; }

        // FK — köhnə Term-lər üçün nullable. Migration ilə bütün köhnə Term-lər
        // avtomatik bir "Default" AcademicYear-a bağlanacaq.
        public int? AcademicYearId { get; set; }
        public AcademicYear? AcademicYear { get; set; }

        public ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();

        /// <summary>
        /// Verilmiş ana görə Term-in cari fazasını qaytarır.
        /// Yeni tarix sahələri null olsa fallback məntiqi işləyir
        /// (köhnə Term-lər üçün geriyə uyğunluq).
        /// </summary>
        public TermPhase GetCurrentPhase(DateTime now)
        {
            var regStart = RegistrationStart ?? StartDate.AddDays(-14);
            var regEnd = RegistrationEnd ?? StartDate.AddDays(-1);
            var examStart = ExamPeriodStart ?? EndDate.AddDays(-14);
            var examEnd = ExamPeriodEnd ?? EndDate;
            var gradeDeadline = GradeSubmissionDeadline ?? EndDate.AddDays(7);

            if (now < regStart) return TermPhase.Planning;
            if (now < regEnd) return TermPhase.Registration;
            if (now < StartDate) return TermPhase.Preparation;
            if (now < examStart) return TermPhase.Active;
            if (now <= examEnd) return TermPhase.ExamPeriod;
            if (now <= gradeDeadline) return TermPhase.GradeReview;
            return TermPhase.Closed;
        }
    }
}
