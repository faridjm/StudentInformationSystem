using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IGradeRepository : IGenericRepository<Grade>
    {
        Task<IEnumerable<Grade>> GetGradesByEnrollmentAsync(int enrollmentId);
        Task<IEnumerable<Grade>> GetGradesByStudentAsync(int studentId);
        Task<decimal> GetAverageScoreAsync(int enrollmentId);
    }
}
