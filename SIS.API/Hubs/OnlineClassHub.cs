using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SIS.BLL.Services.Interfaces;
using System.Security.Claims;

namespace SIS.API.Hubs
{
    /// <summary>
    /// Online classroom üçün SignalR hub.
    /// Yalnız signaling üçün istifadə olunur — audio/video birbaşa peer-to-peer
    /// WebRTC ilə gedir. Eyni zamanda canlı söhbət mesajlarını fan-out edir.
    ///
    /// QEYD: Bu mesh WebRTC demo-dur (1 müəllim + az sayda tələbə). Böyük production
    /// ölçəkləri üçün SFU media server lazımdır (LiveKit / mediasoup və s.) —
    /// bu layihədə implement olunmur.
    /// </summary>
    [Authorize]
    public class OnlineClassHub : Hub
    {
        private readonly IOnlineLessonService _service;

        public OnlineClassHub(IOnlineLessonService service) { _service = service; }

        private string UserId => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        public static string GroupName(int lessonId) => $"online-lesson-{lessonId}";

        // ── Lifecycle ──

        public async Task JoinLesson(int lessonId)
        {
            if (!await _service.CanAccessClassroomAsync(lessonId, UserId))
                throw new HubException("Forbidden");

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(lessonId));

            // Mövcud iştirakçılara yeni connection-u xəbər ver — hər biri öz tərəfdən offer göndərəcək
            await Clients.OthersInGroup(GroupName(lessonId))
                .SendAsync("UserJoined", new { connectionId = Context.ConnectionId, userId = UserId });
        }

        public async Task LeaveLesson(int lessonId)
        {
            await Clients.OthersInGroup(GroupName(lessonId))
                .SendAsync("UserLeft", new { connectionId = Context.ConnectionId, userId = UserId });

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(lessonId));
        }

        // ── WebRTC signaling (target connection) ──

        public Task SendOffer(int lessonId, string targetConnectionId, object offer)
            => Clients.Client(targetConnectionId).SendAsync("ReceiveOffer",
                new { fromConnectionId = Context.ConnectionId, fromUserId = UserId, offer });

        public Task SendAnswer(int lessonId, string targetConnectionId, object answer)
            => Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer",
                new { fromConnectionId = Context.ConnectionId, answer });

        public Task SendIceCandidate(int lessonId, string targetConnectionId, object candidate)
            => Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate",
                new { fromConnectionId = Context.ConnectionId, candidate });

        // ── Classroom chat ──

        public async Task SendClassMessage(int lessonId, string message)
        {
            if (!await _service.CanAccessClassroomAsync(lessonId, UserId))
                throw new HubException("Forbidden");

            var dto = await _service.SaveClassroomChatMessageAsync(lessonId, UserId, message ?? "");
            await Clients.Group(GroupName(lessonId)).SendAsync("ReceiveClassMessage", dto);
        }

        // ── Disconnect ──

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // SignalR group-dan avtomatik çıxarır; peer-lər RTCPeerConnection-u
            // timeout/onclose üzərindən bağlayacaq.
            return base.OnDisconnectedAsync(exception);
        }
    }
}
