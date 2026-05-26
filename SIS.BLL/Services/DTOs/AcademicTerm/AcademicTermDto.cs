using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.AcademicTerm
{
    public class AcademicTermDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        // Yeni sahələr — köhnə Term-lərdə null ola bilər
        public DateTime? RegistrationStart { get; set; }
        public DateTime? RegistrationEnd { get; set; }
        public DateTime? DropDeadline { get; set; }
        public DateTime? ExamPeriodStart { get; set; }
        public DateTime? ExamPeriodEnd { get; set; }
        public DateTime? GradeSubmissionDeadline { get; set; }

        // Sezon məlumatları
        public int? AcademicYearId { get; set; }
        public string? AcademicYearName { get; set; }

        // Service hesablayır
        public TermPhase CurrentPhase { get; set; }
    }
}
