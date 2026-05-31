using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class GradeRepository : GenericRepository<Grade>, IGradeRepository
    {
        public GradeRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Grade>> GetGradesByEnrollmentAsync(int enrollmentId)
            => await _dbSet.Where(g => g.EnrollmentId == enrollmentId).ToListAsync();

        public async Task<IEnumerable<Grade>> GetGradesByStudentAsync(int studentId)
            => await _dbSet
                .Include(g => g.Enrollment).ThenInclude(e => e.Group).ThenInclude(g => g.Course)
                .Where(g => g.Enrollment.StudentId == studentId)
                .ToListAsync();

        public async Task<decimal> GetAverageScoreAsync(int enrollmentId)
        {
            var grades = await _dbSet.Where(g => g.EnrollmentId == enrollmentId).ToListAsync();
            if (!grades.Any()) return 0;
            return grades.Average(g => g.Score / g.MaxScore * 100);
        }
    }
}
