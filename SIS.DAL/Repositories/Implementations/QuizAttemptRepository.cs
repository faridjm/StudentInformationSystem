using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class QuizAttemptRepository : GenericRepository<QuizAttempt>, IQuizAttemptRepository
    {
        public QuizAttemptRepository(AppDbContext context) : base(context) { }

        public async Task<QuizAttempt?> GetByQuizAndEnrollmentAsync(int quizId, int enrollmentId)
            => await _dbSet
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.QuizId == quizId && a.EnrollmentId == enrollmentId);

        public async Task<QuizAttempt?> GetWithAnswersAsync(int attemptId)
            => await _dbSet
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

        public async Task<QuizAttempt?> GetDetailAsync(int attemptId)
            => await _dbSet
                .Include(a => a.Quiz).ThenInclude(q => q.Questions).ThenInclude(qq => qq.Options)
                .Include(a => a.Answers).ThenInclude(an => an.QuizQuestion).ThenInclude(qq => qq.Options)
                .Include(a => a.Enrollment).ThenInclude(e => e.Student).ThenInclude(st => st.AppUser)
                .Include(a => a.Enrollment).ThenInclude(e => e.Group)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

        public async Task<IEnumerable<QuizAttempt>> GetByQuizAsync(int quizId)
            => await _dbSet
                .Include(a => a.Enrollment).ThenInclude(e => e.Student).ThenInclude(st => st.AppUser)
                .Where(a => a.QuizId == quizId)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
    }
}
