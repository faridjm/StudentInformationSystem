using SIS.BLL.Services.DTOs.OnlineLesson;

namespace SIS.BLL.Services.Interfaces
{
    /// <summary>
    /// Online classroom üçün realtime push (SignalR OnlineClassHub-a fan-out).
    /// API layer SignalR HubContext ilə implement edir.
    /// </summary>
    public interface IOnlineClassNotifier
    {
        /// <summary>Müəllim dərsi bitirdi — classroom group-una xəbər ver.</summary>
        Task NotifyLessonEndedAsync(int onlineLessonId);

        /// <summary>Müəllim dərsi ləğv etdi — classroom group-una xəbər ver.</summary>
        Task NotifyLessonCancelledAsync(int onlineLessonId);

        /// <summary>Hub-dan yan keçərək REST endpoint vasitəsilə göndərilən mesajı fan-out et.</summary>
        Task NotifyClassMessageAsync(int onlineLessonId, OnlineLessonChatMessageDto message);
    }
}
