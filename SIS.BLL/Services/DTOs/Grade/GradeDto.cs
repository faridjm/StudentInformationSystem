using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Grade
{
    public class GradeDto
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public GradeType Type { get; set; }
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public string? Note { get; set; }
        public DateTime GradedAt { get; set; }
    }

    public class CreateGradeDto
    {
        public int EnrollmentId { get; set; }
        public GradeType Type { get; set; }
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateGradeDto
    {
        public int Id { get; set; }
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public string? Note { get; set; }
    }
}
