using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(AppDbContext context) : base(context) { }

        public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(
            string? action, string? entityName, string? search,
            DateTime? fromDate, DateTime? toDate, int page, int pageSize)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(a => a.Action == action);

            if (!string.IsNullOrWhiteSpace(entityName))
                query = query.Where(a => a.EntityName == entityName);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(a =>
                    (a.UserFullName != null && a.UserFullName.Contains(search)) ||
                    (a.Description != null && a.Description.Contains(search)) ||
                    (a.EntityId != null && a.EntityId.Contains(search)));

            if (fromDate.HasValue)
                query = query.Where(a => a.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.CreatedAt <= toDate.Value);

            var total = await query.CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<IEnumerable<string>> GetDistinctActionsAsync()
            => await _dbSet.Select(a => a.Action).Distinct().OrderBy(a => a).ToListAsync();

        public async Task<IEnumerable<string>> GetDistinctEntitiesAsync()
            => await _dbSet.Select(a => a.EntityName).Distinct().OrderBy(a => a).ToListAsync();
    }
}
