using Microsoft.EntityFrameworkCore;
using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
            => await _dbSet
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Notification>> GetRecentAsync(string userId, int count)
            => await _dbSet
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();

        public async Task<(IEnumerable<Notification> Items, int TotalCount)> GetPagedAsync(
            string userId, bool? isRead, SIS.Entities.Enums.NotificationType? type, int page, int pageSize)
        {
            var query = _dbSet.Where(n => n.UserId == userId);

            if (isRead.HasValue)
                query = query.Where(n => n.IsRead == isRead.Value);
            if (type.HasValue)
                query = query.Where(n => n.Type == type.Value);

            var total = await query.CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _dbSet
                .CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task<bool> MarkAsReadAsync(int id, string userId)
        {
            var notification = await _dbSet.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (notification == null) return false;
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }
            return true;
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }
        }
    }
}

