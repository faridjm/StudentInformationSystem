using SIS.BLL.Services.DTOs.AuditLog;
using SIS.BLL.Services.DTOs.Common;

namespace SIS.BLL.Services.Interfaces
{
    public interface IAuditLogService
    {
        /// <summary>
        /// Audit qeydi yaradır. Həssas məlumat (parol, token və s.) ötürülməməlidir.
        /// Loglama xətası əsas əməliyyatı pozmur.
        /// </summary>
        Task LogAsync(string action, string entityName, string? entityId,
                      string? description = null, object? oldValue = null, object? newValue = null);

        Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogFilterDto filter);
        Task<IEnumerable<string>> GetActionsAsync();
        Task<IEnumerable<string>> GetEntitiesAsync();
    }
}
