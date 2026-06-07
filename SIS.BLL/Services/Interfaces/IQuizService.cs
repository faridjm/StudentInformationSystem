using SIS.BLL.Services.DTOs.Quiz;

namespace SIS.BLL.Services.Interfaces
{
    public interface IQuizService
    {
        // Teacher — manage
        Task<QuizDto?> GetByAssessmentItemAsync(int assessmentItemId);
        Task<QuizDto?> GetByIdAsync(int id);
        Task<QuizDetailDto?> GetDetailAsync(int id);
        Task<IEnumerable<QuizDto>> GetByGroupAsync(int groupId);
        Task<IEnumerable<QuizDto>> GetActiveByGroupAsync(int groupId);
        Task<QuizDto> CreateAsync(CreateQuizDto dto);
        Task UpdateAsync(UpdateQuizDto dto);
        Task DeleteAsync(int id);
        Task SetActiveAsync(int id, bool active);
        Task<QuizQuestionDto> AddQuestionAsync(int quizId, CreateQuizQuestionDto dto);
        Task DeleteQuestionAsync(int quizId, int questionId);
        Task<IEnumerable<QuizResultDto>> GetResultsAsync(int quizId);

        /// <summary>Tək cəhd üçün sual-sual detallı nəticə (Teacher-of-group və ya owning Student).</summary>
        Task<QuizAttemptDetailDto?> GetAttemptDetailAsync(int attemptId, string requestingUserId);

        // Student — take
        Task<TakeQuizDto> StartAttemptAsync(int quizId, int enrollmentId);
        Task<QuizResultDto> SubmitAttemptAsync(SubmitQuizDto dto);
        Task<QuizResultDto?> GetMyResultAsync(int quizId, int enrollmentId);
    }
}
