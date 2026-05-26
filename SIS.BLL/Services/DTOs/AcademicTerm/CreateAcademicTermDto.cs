namespace SIS.BLL.Services.DTOs.AcademicTerm
{
    public class CreateAcademicTermDto
    {
        public string Name { get; set; } = null!;

        /// <summary>Hansı dərs sezonuna aiddir.</summary>
        public int AcademicYearId { get; set; }

        /// <summary>Tələbə qeydiyyatı açılır.</summary>
        public DateTime RegistrationStart { get; set; }

        /// <summary>Tələbə qeydiyyatı bağlanır.</summary>
        public DateTime RegistrationEnd { get; set; }

        /// <summary>Dərslərin başladığı gün.</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Bu tarixdən sonra kursdan çıxma F qiymət olur.</summary>
        public DateTime DropDeadline { get; set; }

        /// <summary>Dərslərin bitdiyi gün.</summary>
        public DateTime EndDate { get; set; }

        /// <summary>İmtahan dövrü başlayır.</summary>
        public DateTime ExamPeriodStart { get; set; }

        /// <summary>İmtahan dövrü bitir.</summary>
        public DateTime ExamPeriodEnd { get; set; }

        /// <summary>Müəllimin qiymət düzəlişi son tarixi.</summary>
        public DateTime GradeSubmissionDeadline { get; set; }
    }
}
