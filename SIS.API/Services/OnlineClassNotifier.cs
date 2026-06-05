using Microsoft.AspNetCore.SignalR;
using SIS.API.Hubs;
using SIS.BLL.Services.DTOs.OnlineLesson;
using SIS.BLL.Services.Interfaces;

namespace SIS.API.Services
{
    /// <summary>
    /// OnlineClassHub-a fan-out edən SignalR notifier.
    /// REST endpoint-lərdən (məs. EndOnlineLesson) çağırılır ki, classroom-da
    /// olan peer-lərə real-time event çatsın.
    /// </summary>
    public class OnlineClassNotifier : IOnlineClassNotifier
    {
        private readonly IHubContext<OnlineClassHub> _hub;
        public OnlineClassNotifier(IHubContext<OnlineClassHub> hub) { _hub = hub; }

        public Task NotifyLessonEndedAsync(int onlineLessonId)
            => _hub.Clients.Group(OnlineClassHub.GroupName(onlineLessonId))
                .SendAsync("LessonEnded", new { lessonId = onlineLessonId });

        public Task NotifyLessonCancelledAsync(int onlineLessonId)
            => _hub.Clients.Group(OnlineClassHub.GroupName(onlineLessonId))
                .SendAsync("LessonCancelled", new { lessonId = onlineLessonId });

        public Task NotifyClassMessageAsync(int onlineLessonId, OnlineLessonChatMessageDto message)
            => _hub.Clients.Group(OnlineClassHub.GroupName(onlineLessonId))
                .SendAsync("ReceiveClassMessage", message);
    }
}
