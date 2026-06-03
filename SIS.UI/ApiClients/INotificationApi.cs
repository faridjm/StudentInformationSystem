using Refit;
using SIS.BLL.Services.DTOs.Common;
using SIS.BLL.Services.DTOs.Notification;
using SIS.Entities.Enums;

namespace SIS.UI.ApiClients
{
    public interface INotificationApi
    {
        [Get("/api/notification")]
        Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync();

        [Get("/api/notification/recent")]
        Task<IEnumerable<NotificationDto>> GetRecentAsync(int count);

        [Get("/api/notification/paged")]
        Task<PagedResult<NotificationDto>> GetPagedAsync(bool? isRead, NotificationType? type, int page, int pageSize);

        [Get("/api/notification/unread-count")]
        Task<UnreadCountDto> GetUnreadCountAsync();

        [Put("/api/notification/{id}/read")]
        Task MarkAsReadAsync(int id);

        [Put("/api/notification/mark-all-read")]
        Task MarkAllAsReadAsync();
    }
}
