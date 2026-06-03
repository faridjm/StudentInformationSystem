using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IQuizRepository : IGenericRepository<Quiz>
    {
        Task<Quiz?> GetByAssessmentItemAsync(int assessmentItemId);
        Task<Quiz?> GetWithQuestionsAsync(int quizId);
        Task<Quiz?> GetByIdWithCountsAsync(int quizId);
        Task<IEnumerable<Quiz>> GetByGroupAsync(int groupId);
    }
}
