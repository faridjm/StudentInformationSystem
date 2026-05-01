using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IActivityLogRepository : IGenericRepository<ActivityLog>
    {
        Task<IEnumerable<ActivityLog>> GetLogsByUserAsync(string userId);
        Task<IEnumerable<ActivityLog>> GetLogsByEntityAsync(string entityName);
        Task<IEnumerable<ActivityLog>> GetRecentLogsAsync(int count);
    }
}
