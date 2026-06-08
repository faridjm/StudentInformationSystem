namespace SIS.BLL.Services.DTOs.Transcript
{
    public class TranscriptDto
    {
        // Student
        public string StudentName { get; set; } = null!;
        public string StudentNumber { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int EnrollmentYear { get; set; }

        // Course / Group
        public string CourseName { get; set; } = null!;
        public string GroupName { get; set; } = null!;
        public string TeacherName { get; set; } = null!;
        public string? AcademicYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Attendance
        public int TotalSessions { get; set; }
        public int AttendanceTakenSessions { get; set; }
        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int ExcusedCount { get; set; }
        public double AttendancePercentage { get; set; }

        // Official Assessments (GPA-ya təsir edir)
        public List<TranscriptAssessmentDto> OfficialAssessments { get; set; } = new();
        public decimal OfficialGrade { get; set; } // Σ((Score/MaxScore) * WeightPercent)
        public string LetterGrade { get; set; } = null!;
        public decimal GPA { get; set; }

        // Homework Assignments (GPA-ya təsir etmir)
        public List<TranscriptAssignmentDto> HomeworkAssignments { get; set; } = new();
        public decimal? HomeworkAverage { get; set; }

        // Status
        public string PerformanceStatus { get; set; } = null!; // Excellent / Good / Satisfactory / Failing
        public bool CertificateEligible { get; set; }
        public DateTime GeneratedAt { get; set; }

        // ── Transcript result (Progress / Final) — yeni status təbəqəsi ──
        // Calculation dəyişmir; bu sahələr TranscriptResultCalculator-dan gəlir.
        public SIS.Entities.Enums.TranscriptType TranscriptType { get; set; }
        public bool IsCourseCompleted { get; set; }
        public string CompletionStatus { get; set; } = "InProgress"; // Completed / InProgress
        public string CompletionLevel { get; set; } = "Pending";     // Pending/NotCompleted/NormalCompletion/HonorCompletion/HighHonorCompletion/NotEligible
        public string CertificateStatus { get; set; } = "Pending";   // Pending/Eligible/NotEligible

        // Identifikasiya üçün (controller-də doldurulur)
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int GroupId { get; set; }

        // ── QR doğrulama (PDF generasiya anında controller/service tərəfindən doldurulur) ──
        public string? VerificationCode { get; set; }
        public string? VerificationUrl { get; set; }
        public DateTime? VerificationIssuedAt { get; set; }
        public DateTime? VerificationExpiresAt { get; set; }
    }

    public class TranscriptAssessmentDto
    {
        public string Title { get; set; } = null!;
        public string Type { get; set; } = null!; // Quiz, Assignment, FinalProject
        public DateTime Date { get; set; }
        public decimal MaxScore { get; set; }
        public decimal WeightPercent { get; set; }
        public decimal? Score { get; set; }
        public bool IsGraded { get; set; }
        public decimal? Percentage => IsGraded && MaxScore > 0 ? Score / MaxScore * 100 : null;
        public decimal? WeightedScore => IsGraded && MaxScore > 0 ? Score / MaxScore * WeightPercent : null;
    }

    public class TranscriptAssignmentDto
    {
        public string Title { get; set; } = null!;
        public DateTime DueDate { get; set; }
        public decimal MaxScore { get; set; }
        public decimal? Score { get; set; }
        public bool IsGraded { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsLinkedToAssessment { get; set; }
        public decimal? Percentage => IsGraded && MaxScore > 0 ? Score / MaxScore * 100 : null;
    }
}
