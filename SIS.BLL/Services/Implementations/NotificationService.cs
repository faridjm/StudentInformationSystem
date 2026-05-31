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
            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id, Title = n.Title, Message = n.Message,
                Type = n.Type, IsRead = n.IsRead, CreatedAt = n.CreatedAt
            });
        }

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _unitOfWork.Notifications.GetUnreadCountAsync(userId);

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            await _unitOfWork.SaveAsync();
        }

        public async Task SendNotificationAsync(string userId, string title, string message, NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId, Title = title, Message = message,
                Type = type, IsRead = false, CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveAsync();
            await _realtimeNotifier.NotifyUserAsync(userId, title, message, type.ToString());
        }
    }
}
