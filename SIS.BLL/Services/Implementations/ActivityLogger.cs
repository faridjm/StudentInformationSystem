using Microsoft.AspNetCore.Http;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Models;
using System.Security.Claims;
using System.Text.Json;

namespace SIS.BLL.Services.Implementations
{
    public class ActivityLogger : IActivityLogger
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActivityLogger(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string entityName, string? entityId,
                                   object? oldValue, object? newValue)
        {
            var context = _httpContextAccessor.HttpContext;

            var userId = context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var ipAddress = context?.Connection?.RemoteIpAddress?.ToString();

            var log = new ActivityLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValue = oldValue != null ? JsonSerializer.Serialize(oldValue) : null,
                NewValue = newValue != null ? JsonSerializer.Serialize(newValue) : null,
                UserId = userId,
                IpAddress = ipAddress
            };

            await _unitOfWork.ActivityLogs.AddAsync(log);
            await _unitOfWork.SaveAsync();
        }
    }
}
