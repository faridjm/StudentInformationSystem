using Refit;
using SIS.BLL.Services.DTOs.AuditLog;
using SIS.BLL.Services.DTOs.Common;

namespace SIS.UI.ApiClients
{
    public interface IAuditLogApi
    {
        [Get("/api/auditlog")]
        Task<PagedResult<AuditLogDto>> GetPagedAsync(
            string? action, string? entityName, string? search,
            DateTime? fromDate, DateTime? toDate, int page, int pageSize);

        [Get("/api/auditlog/actions")]
        Task<IEnumerable<string>> GetActionsAsync();

        [Get("/api/auditlog/entities")]
        Task<IEnumerable<string>> GetEntitiesAsync();
    }
}
