using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(
            string? action, string? entityName, string? search,
            DateTime? fromDate, DateTime? toDate, int page, int pageSize);

        Task<IEnumerable<string>> GetDistinctActionsAsync();
        Task<IEnumerable<string>> GetDistinctEntitiesAsync();
    }
}
