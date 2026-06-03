using Refit;
using SIS.BLL.Services.DTOs.Quiz;

namespace SIS.UI.ApiClients
{
    public interface IQuizApi
    {
        [Get("/api/quiz/{id}")]
        Task<QuizDto> GetByIdAsync(int id);

        [Get("/api/quiz/{id}/detail")]
        Task<QuizDetailDto> GetDetailAsync(int id);

        [Get("/api/quiz/by-assessment/{assessmentItemId}")]
        Task<QuizDto> GetByAssessmentAsync(int assessmentItemId);

        [Get("/api/quiz/group/{groupId}")]
        Task<IEnumerable<QuizDto>> GetByGroupAsync(int groupId);

        [Get("/api/quiz/group/{groupId}/active")]
        Task<IEnumerable<QuizDto>> GetActiveByGroupAsync(int groupId);

        [Get("/api/quiz/{id}/results")]
        Task<IEnumerable<QuizResultDto>> GetResultsAsync(int id);

        [Post("/api/quiz")]
        Task<QuizDto> CreateAsync([Body] CreateQuizDto dto);

        [Put("/api/quiz")]
        Task UpdateAsync([Body] UpdateQuizDto dto);

        [Delete("/api/quiz/{id}")]
        Task DeleteAsync(int id);

        [Post("/api/quiz/{id}/activate")]
        Task SetActiveAsync(int id, bool active);

        [Post("/api/quiz/{id}/questions")]
        Task<QuizQuestionDto> AddQuestionAsync(int id, [Body] CreateQuizQuestionDto dto);

        [Delete("/api/quiz/{quizId}/questions/{questionId}")]
        Task DeleteQuestionAsync(int quizId, int questionId);

        [Get("/api/quiz/{quizId}/start/{enrollmentId}")]
        Task<TakeQuizDto> StartAsync(int quizId, int enrollmentId);

        [Post("/api/quiz/submit")]
        Task<QuizResultDto> SubmitAsync([Body] SubmitQuizDto dto);

        [Get("/api/quiz/{quizId}/my-result/{enrollmentId}")]
        Task<QuizResultDto> GetMyResultAsync(int quizId, int enrollmentId);
    }
}
