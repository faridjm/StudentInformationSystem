using SIS.BLL.Services.DTOs.Common;
using SIS.BLL.Services.DTOs.Notification;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;

namespace SIS.BLL.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRealtimeNotifier _realtimeNotifier;

        public NotificationService(IUnitOfWork unitOfWork, IRealtimeNotifier realtimeNotifier)
        {
            _unitOfWork = unitOfWork;
            _realtimeNotifier = realtimeNotifier;
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _unitOfWork.Notifications.GetUserNotificationsAsync(userId);
            return notifications.Select(MapToDto);
        }

        public async Task<IEnumerable<NotificationDto>> GetRecentAsync(string userId, int count)
        {
            var notifications = await _unitOfWork.Notifications.GetRecentAsync(userId, count);
            return notifications.Select(MapToDto);
        }

        public async Task<PagedResult<NotificationDto>> GetPagedAsync(
            string userId, bool? isRead, NotificationType? type, int page, int pageSize)
        {
            var (items, total) = await _unitOfWork.Notifications.GetPagedAsync(userId, isRead, type, page, pageSize);
            return new PagedResult<NotificationDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = total,
                Page = page < 1 ? 1 : page,
                PageSize = pageSize < 1 ? 20 : pageSize
            };
        }

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _unitOfWork.Notifications.GetUnreadCountAsync(userId);

        public async Task<bool> MarkAsReadAsync(int id, string userId)
        {
            var ok = await _unitOfWork.Notifications.MarkAsReadAsync(id, userId);
            if (ok) await _unitOfWork.SaveAsync();
            return ok;
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            await _unitOfWork.SaveAsync();
        }

        public async Task SendNotificationAsync(string userId, string title, string message, NotificationType type, string? relatedUrl = null)
        {
            if (string.IsNullOrEmpty(userId)) return;

            var notification = new Notification
            {
                UserId = userId, Title = title, Message = message,
                Type = type, RelatedUrl = relatedUrl, IsRead = false, CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveAsync();

            try { await _realtimeNotifier.NotifyUserAsync(userId, title, message, type.ToString()); }
            catch { /* realtime xətası əsas axını pozmasın */ }
        }

        public async Task SendToManyAsync(IEnumerable<string> userIds, string title, string message, NotificationType type, string? relatedUrl = null)
        {
            var ids = userIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            if (ids.Count == 0) return;

            var now = DateTime.UtcNow;
            foreach (var id in ids)
            {
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserId = id, Title = title, Message = message,
                    Type = type, RelatedUrl = relatedUrl, IsRead = false, CreatedAt = now
                });
            }
            await _unitOfWork.SaveAsync();

            foreach (var id in ids)
            {
                try { await _realtimeNotifier.NotifyUserAsync(id, title, message, type.ToString()); }
                catch { }
            }
        }

        private static NotificationDto MapToDto(Notification n) => new()
        {
            Id = n.Id, Title = n.Title, Message = n.Message,
            Type = n.Type, IsRead = n.IsRead, RelatedUrl = n.RelatedUrl,
            CreatedAt = n.CreatedAt, ReadAt = n.ReadAt
        };
    }
}
