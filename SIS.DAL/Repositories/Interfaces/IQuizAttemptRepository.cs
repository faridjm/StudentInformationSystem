using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IQuizAttemptRepository : IGenericRepository<QuizAttempt>
    {
        Task<QuizAttempt?> GetByQuizAndEnrollmentAsync(int quizId, int enrollmentId);
        Task<QuizAttempt?> GetWithAnswersAsync(int attemptId);
        Task<QuizAttempt?> GetDetailAsync(int attemptId);
        Task<IEnumerable<QuizAttempt>> GetByQuizAsync(int quizId);
    }
}
