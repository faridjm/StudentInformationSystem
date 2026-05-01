using SIS.BLL.Services.DTOs.Notification;

namespace SIS.BLL.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAllAsReadAsync(string userId);
        Task SendNotificationAsync(string userId, string title, string message, SIS.Entities.Enums.NotificationType type);
    }
}
