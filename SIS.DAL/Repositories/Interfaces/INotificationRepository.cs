using SIS.Entities.Enums;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
        Task<IEnumerable<Notification>> GetRecentAsync(string userId, int count);
        Task<(IEnumerable<Notification> Items, int TotalCount)> GetPagedAsync(
            string userId, bool? isRead, NotificationType? type, int page, int pageSize);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int id, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}
