using SIS.Entities.Enums;

namespace SIS.BLL.Services.DTOs.Quiz
{
    // ── Teacher: list/summary ────────────────────────────────────────
    public class QuizDto
    {
        public int Id { get; set; }
        public int AssessmentItemId { get; set; }
        public string AssessmentItemTitle { get; set; } = null!;
        public int GroupId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? DurationMinutes { get; set; }
        public decimal MaxScore { get; set; }
        public decimal WeightPercent { get; set; }
        public int QuestionCount { get; set; }
        public int AttemptCount { get; set; }
    }

    // ── Teacher: full detail (correct answers visible) ───────────────
    public class QuizDetailDto
    {
        public int Id { get; set; }
        public int AssessmentItemId { get; set; }
        public string AssessmentItemTitle { get; set; } = null!;
        public int GroupId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? DurationMinutes { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    public class QuizQuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public QuestionType Type { get; set; }
        public decimal Points { get; set; }
        public int Order { get; set; }
        public List<QuizOptionDto> Options { get; set; } = new();
    }

    public class QuizOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }
        public int Order { get; set; }
    }

    // ── Create / Update ──────────────────────────────────────────────
    public class CreateQuizDto
    {
        public int AssessmentItemId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? DurationMinutes { get; set; }
        public List<CreateQuizQuestionDto> Questions { get; set; } = new();
    }

    public class CreateQuizQuestionDto
    {
        public string Text { get; set; } = null!;
        public QuestionType Type { get; set; }
        public decimal Points { get; set; } = 1;
        public List<CreateQuizOptionDto> Options { get; set; } = new();
    }

    public class CreateQuizOptionDto
    {
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }

    public class UpdateQuizDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? DurationMinutes { get; set; }
    }

    // ── Student: take quiz (no correct answers exposed) ──────────────
    public class TakeQuizDto
    {
        public int QuizId { get; set; }
        public int AttemptId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string AssessmentItemTitle { get; set; } = null!;
        public int EnrollmentId { get; set; }
        public int? DurationMinutes { get; set; }
        public DateTime StartedAt { get; set; }
        public bool AlreadySubmitted { get; set; }
        public decimal? MyScore { get; set; }
        public List<TakeQuestionDto> Questions { get; set; } = new();
    }

    public class TakeQuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public QuestionType Type { get; set; }
        public decimal Points { get; set; }
        public int Order { get; set; }
        public List<TakeOptionDto> Options { get; set; } = new();
    }

    public class TakeOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public int Order { get; set; }
    }

    // ── Student: submit ──────────────────────────────────────────────
    public class SubmitQuizDto
    {
        public int QuizId { get; set; }
        public int EnrollmentId { get; set; }
        public List<SubmitAnswerDto> Answers { get; set; } = new();
    }

    public class SubmitAnswerDto
    {
        public int QuizQuestionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();
    }

    // ── Result (teacher results + student feedback) ──────────────────
    public class QuizResultDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int EnrollmentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string StudentNumber { get; set; } = null!;
        public decimal? Score { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    // ── Detallı nəticə (sual-sual, doğru/səhv, seçim/correct option) ──
    public class QuizAttemptDetailDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = "";
        public int EnrollmentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentNumber { get; set; } = "";
        public decimal? Score { get; set; }
        public DateTime? SubmittedAt { get; set; }

        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        /// <summary>Boş buraxılmış suallar (heç bir variant seçilməyib).</summary>
        public int EmptyCount { get; set; }

        public List<QuizQuestionResultDto> Questions { get; set; } = new();
    }

    public class QuizQuestionResultDto
    {
        public int QuestionId { get; set; }
        public int Order { get; set; }
        public string Text { get; set; } = "";
        public decimal Points { get; set; }
        public decimal PointsAwarded { get; set; }
        public bool IsCorrect { get; set; }

        /// <summary>Tələbənin seçdiyi variantların id-ləri.</summary>
        public List<int> SelectedOptionIds { get; set; } = new();
        public List<int> CorrectOptionIds { get; set; } = new();

        public List<QuizOptionResultDto> Options { get; set; } = new();
    }

    public class QuizOptionResultDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public bool IsCorrect { get; set; }
        public bool WasSelected { get; set; }
    }
}
