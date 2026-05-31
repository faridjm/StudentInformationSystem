using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IClassSessionRepository : IGenericRepository<ClassSession>
    {
        Task<IEnumerable<ClassSession>> GetByGroupAsync(int groupId);
        Task<IEnumerable<ClassSession>> GetByDateRangeAsync(int groupId, DateOnly from, DateOnly to);
    }
}
