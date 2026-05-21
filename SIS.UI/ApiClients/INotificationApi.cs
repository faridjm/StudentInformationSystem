using Refit;
using SIS.BLL.Services.DTOs.Notification;

namespace SIS.UI.ApiClients
{
    public interface INotificationApi
    {
        [Get("/api/notification")]
        Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync();

        [Get("/api/notification/unread-count")]
        Task<UnreadCountDto> GetUnreadCountAsync();

        [Put("/api/notification/mark-all-read")]
        Task MarkAllAsReadAsync();
    }


}
