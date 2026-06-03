using SIS.BLL.Services.DTOs.Common;
using SIS.BLL.Services.DTOs.Notification;
using SIS.Entities.Enums;

namespace SIS.BLL.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);
        Task<IEnumerable<NotificationDto>> GetRecentAsync(string userId, int count);
        Task<PagedResult<NotificationDto>> GetPagedAsync(string userId, bool? isRead, NotificationType? type, int page, int pageSize);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int id, string userId);
        Task MarkAllAsReadAsync(string userId);
        Task SendNotificationAsync(string userId, string title, string message, NotificationType type, string? relatedUrl = null);
        Task SendToManyAsync(IEnumerable<string> userIds, string title, string message, NotificationType type, string? relatedUrl = null);
    }
}
