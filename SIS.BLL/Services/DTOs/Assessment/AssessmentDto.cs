using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Assessment
{
    public class AssessmentItemDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public AssessmentType Type { get; set; }
        public string Title { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal MaxScore { get; set; }
        public decimal WeightPercent { get; set; }
        public string? Description { get; set; }
        public bool IsVisible { get; set; }
        public int SubmissionCount { get; set; }
        public int GradedCount { get; set; }
    }

    public class CreateAssessmentItemDto
    {
        public int GroupId { get; set; }
        public AssessmentType Type { get; set; }
        public string Title { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal MaxScore { get; set; }
        public decimal WeightPercent { get; set; }
        public string? Description { get; set; }
        public bool IsVisible { get; set; } = true;
    }

    public class UpdateAssessmentItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal MaxScore { get; set; }
        public decimal WeightPercent { get; set; }
        public string? Description { get; set; }
        public bool IsVisible { get; set; }
    }

    public class AssessmentSubmissionDto
    {
        public int Id { get; set; }
        public int AssessmentItemId { get; set; }
        public string AssessmentTitle { get; set; } = null!;
        public int EnrollmentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string StudentNumber { get; set; } = null!;
        public decimal? Score { get; set; }
        public decimal MaxScore { get; set; }
        public string? Feedback { get; set; }
        public bool IsGraded { get; set; }
        public DateTime? GradedAt { get; set; }
    }

    public class GradeAssessmentDto
    {
        public int SubmissionId { get; set; }
        public decimal Score { get; set; }
        public string? Feedback { get; set; }
    }
}
