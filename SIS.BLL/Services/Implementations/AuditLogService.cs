using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SIS.BLL.Services.DTOs.AuditLog;
using SIS.BLL.Services.DTOs.Common;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using System.Security.Claims;
using System.Text.Json;

namespace SIS.BLL.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditLogService>? _logger;

        public AuditLogService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor,
            ILogger<AuditLogService>? logger = null)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAsync(string action, string entityName, string? entityId,
            string? description = null, object? oldValue = null, object? newValue = null)
        {
            try
            {
                var ctx = _httpContextAccessor.HttpContext;
                var user = ctx?.User;

                // ClaimTypes.* (long form) ilkin axtarış; əgər boşdursa fallback olaraq
                // qısa formaları (raw "nameid", "role", "unique_name") da yoxla.
                string? userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? user?.FindFirst("nameid")?.Value
                                 ?? user?.FindFirst("sub")?.Value;
                string? userName = user?.FindFirst(ClaimTypes.Name)?.Value
                                   ?? user?.FindFirst("unique_name")?.Value
                                   ?? user?.FindFirst("name")?.Value;
                string? userRole = user?.FindFirst(ClaimTypes.Role)?.Value
                                   ?? user?.FindFirst("role")?.Value;

                var log = new AuditLog
                {
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    Description = Truncate(description, 1000),
                    UserId = userId,
                    UserFullName = userName,
                    UserRole = userRole,
                    IpAddress = GetIpAddress(ctx),
                    UserAgent = Truncate(ctx?.Request?.Headers["User-Agent"].ToString(), 512),
                    OldValue = Serialize(oldValue),
                    NewValue = Serialize(newValue),
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.AuditLogs.AddAsync(log);
                await _unitOfWork.SaveAsync();

                // Diaqnostik məqsədilə: uğurlu yazılmadan sonra konsola informasiya
                _logger?.LogInformation("AuditLog yazıldı: {Action} {Entity} #{EntityId} (user: {UserName})",
                    action, entityName, entityId ?? "-", userName ?? "anon");
            }
            catch (Exception ex)
            {
                // Audit loglaması heç vaxt əsas əməliyyatı pozmamalıdır,
                // amma development-də sakitcə uduzmasın — ILogger ilə qeyd et.
                _logger?.LogError(ex, "AuditLog YAZILA BİLMƏDİ: {Action} {Entity} {EntityId} — {Msg}",
                    action, entityName, entityId, ex.Message);
                // Console-a da yaz ki, logger configure olmasa belə görünsün
                Console.Error.WriteLine($"[AUDIT FAIL] {action} {entityName} {entityId}: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                    Console.Error.WriteLine($"[AUDIT FAIL] Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
        }

        public async Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogFilterDto filter)
        {
            var (items, total) = await _unitOfWork.AuditLogs.GetPagedAsync(
                filter.Action, filter.EntityName, filter.Search,
                filter.FromDate, filter.ToDate, filter.Page, filter.PageSize);

            return new PagedResult<AuditLogDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = total,
                Page = filter.Page < 1 ? 1 : filter.Page,
                PageSize = filter.PageSize < 1 ? 20 : filter.PageSize
            };
        }

        public async Task<IEnumerable<string>> GetActionsAsync()
            => await _unitOfWork.AuditLogs.GetDistinctActionsAsync();

        public async Task<IEnumerable<string>> GetEntitiesAsync()
            => await _unitOfWork.AuditLogs.GetDistinctEntitiesAsync();

        private static string? GetIpAddress(HttpContext? ctx)
        {
            if (ctx == null) return null;
            var forwarded = ctx.Request?.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(forwarded))
                return forwarded.Split(',')[0].Trim();
            return ctx.Connection?.RemoteIpAddress?.ToString();
        }

        private static string? Serialize(object? value)
        {
            if (value == null) return null;
            try { return JsonSerializer.Serialize(value); }
            catch { return null; }
        }

        private static string? Truncate(string? value, int max)
            => string.IsNullOrEmpty(value) ? value : (value.Length <= max ? value : value.Substring(0, max));

        private static AuditLogDto MapToDto(AuditLog a) => new()
        {
            Id = a.Id,
            UserId = a.UserId,
            UserFullName = a.UserFullName,
            UserRole = a.UserRole,
            Action = a.Action,
            EntityName = a.EntityName,
            EntityId = a.EntityId,
            Description = a.Description,
            OldValue = a.OldValue,
            NewValue = a.NewValue,
            IpAddress = a.IpAddress,
            UserAgent = a.UserAgent,
            CreatedAt = a.CreatedAt
        };
    }
}
