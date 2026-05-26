namespace SIS.BLL.Services.DTOs.AcademicTerm
{
    public class UpdateAcademicTermDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int AcademicYearId { get; set; }
        public DateTime RegistrationStart { get; set; }
        public DateTime RegistrationEnd { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DropDeadline { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ExamPeriodStart { get; set; }
        public DateTime ExamPeriodEnd { get; set; }
        public DateTime GradeSubmissionDeadline { get; set; }
    }
}
