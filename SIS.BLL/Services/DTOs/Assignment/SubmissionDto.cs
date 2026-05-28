namespace SIS.BLL.Services.DTOs.Assignment
{
    public class SubmissionDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = null!;
        public int EnrollmentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string StudentNumber { get; set; } = null!;
        public string? Comment { get; set; }
        public string? FileUrl { get; set; }
        public DateTime SubmittedAt { get; set; }
        public decimal? Score { get; set; }
        public string? Feedback { get; set; }
        public bool IsGraded { get; set; }
        public decimal MaxScore { get; set; }
    }

    public class CreateSubmissionDto
    {
        public int AssignmentId { get; set; }
        public int EnrollmentId { get; set; }
        public string? Comment { get; set; }
        public string? FileUrl { get; set; }
    }

    public class GradeSubmissionDto
    {
        public int SubmissionId { get; set; }
        public decimal Score { get; set; }
        public string? Feedback { get; set; }
    }
}
