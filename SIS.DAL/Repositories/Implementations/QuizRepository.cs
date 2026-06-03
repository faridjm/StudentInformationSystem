using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
    {
        public QuizRepository(AppDbContext context) : base(context) { }

        public async Task<Quiz?> GetByAssessmentItemAsync(int assessmentItemId)
            => await _dbSet
                .FirstOrDefaultAsync(q => q.AssessmentItemId == assessmentItemId);

        public async Task<Quiz?> GetWithQuestionsAsync(int quizId)
            => await _dbSet
                .Include(q => q.AssessmentItem)
                .Include(q => q.Questions.OrderBy(x => x.Order))
                    .ThenInclude(x => x.Options.OrderBy(o => o.Order))
                .FirstOrDefaultAsync(q => q.Id == quizId);

        public async Task<Quiz?> GetByIdWithCountsAsync(int quizId)
            => await _dbSet
                .Include(q => q.AssessmentItem)
                .Include(q => q.Questions)
                .Include(q => q.Attempts)
                .FirstOrDefaultAsync(q => q.Id == quizId);

        public async Task<IEnumerable<Quiz>> GetByGroupAsync(int groupId)
            => await _dbSet
                .Include(q => q.AssessmentItem)
                .Include(q => q.Questions)
                .Include(q => q.Attempts)
                .Where(q => q.GroupId == groupId)
                .OrderBy(q => q.AssessmentItem.Date)
                .ToListAsync();
    }
}
