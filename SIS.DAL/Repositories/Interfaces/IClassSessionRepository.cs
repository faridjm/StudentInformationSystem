using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IClassSessionRepository : IGenericRepository<ClassSession>
    {
        Task<ClassSession?> GetWithDetailsAsync(int id);
        Task<IEnumerable<ClassSession>> GetByCourseOfferingAsync(int courseOfferingId);
        Task<IEnumerable<ClassSession>> GetByCourseOfferingAndDateRangeAsync(
            int courseOfferingId, DateOnly from, DateOnly to);
        Task<IEnumerable<ClassSession>> GetByStudentAndDateRangeAsync(
            int studentId, DateOnly from, DateOnly to);
        Task<bool> ExistsAsync(int courseOfferingId, DateOnly date, TimeOnly startTime);
    }
}
