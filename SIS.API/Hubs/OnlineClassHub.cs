using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SIS.BLL.Services.Interfaces;
using System.Collections.Concurrent;
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

        // Hər dərsdə eyni anda yalnız 1 nəfər ekran paylaşa bilər.
        // lessonId -> (userId, connectionId, fullName). Memory state (migration tələb etmir).
        private static readonly ConcurrentDictionary<int, ScreenShareState> _screenSharers = new();

        public OnlineClassHub(IOnlineLessonService service) { _service = service; }

        private string UserId => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        private string UserFullName => Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "İştirakçı";
        private string UserRole => Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
        public static string GroupName(int lessonId) => $"online-lesson-{lessonId}";

        private sealed record ScreenShareState(string UserId, string ConnectionId, string FullName);

        // ── Lifecycle ──

        public async Task JoinLesson(int lessonId)
        {
            if (!await _service.CanAccessClassroomAsync(lessonId, UserId))
                throw new HubException("Forbidden");

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(lessonId));

            // Mövcud iştirakçılara yeni connection-u xəbər ver — hər biri öz tərəfdən offer göndərəcək
            await Clients.OthersInGroup(GroupName(lessonId))
                .SendAsync("UserJoined", new { connectionId = Context.ConnectionId, userId = UserId, userFullName = UserFullName, userRole = UserRole });

            // Yeni qoşulan üçün — əgər kimsə artıq ekran paylaşırsa, onu məlumatlandır
            if (_screenSharers.TryGetValue(lessonId, out var sharer))
            {
                await Clients.Caller.SendAsync("UserStartedScreenShare", new
                {
                    lessonId,
                    connectionId = sharer.ConnectionId,
                    userId = sharer.UserId,
                    userFullName = sharer.FullName
                });
            }
        }

        public async Task LeaveLesson(int lessonId)
        {
            await Clients.OthersInGroup(GroupName(lessonId))
                .SendAsync("UserLeft", new { connectionId = Context.ConnectionId, userId = UserId });

            // Əgər çıxan iştirakçı ekran paylaşırdısa, statusu təmizlə və xəbər ver
            ClearScreenShareIfOwner(lessonId, Context.ConnectionId, notifyGroup: true);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(lessonId));
        }

        // ── Screen share signaling ──

        /// <summary>
        /// İstifadəçi ekran paylaşmağa başlayır. Eyni anda yalnız 1 nəfər paylaşa bilər.
        /// </summary>
        public async Task StartScreenShare(int lessonId)
        {
            if (!await _service.CanAccessClassroomAsync(lessonId, UserId))
                throw new HubException("Forbidden");

            var state = new ScreenShareState(UserId, Context.ConnectionId, UserFullName);

            // Yalnız heç kim paylaşmırsa əlavə et (atomic)
            var added = _screenSharers.TryAdd(lessonId, state);
            if (!added)
            {
                // Artıq başqası paylaşır — caller-a xəbər ver (öz təkrar çağırışı istisna)
                if (_screenSharers.TryGetValue(lessonId, out var existing) &&
                    existing.ConnectionId != Context.ConnectionId)
                {
                    await Clients.Caller.SendAsync("ScreenShareDenied", new
                    {
                        lessonId,
                        reason = "AlreadySharing",
                        currentSharerName = existing.FullName
                    });
                    return;
                }
            }

            await Clients.Group(GroupName(lessonId)).SendAsync("UserStartedScreenShare", new
            {
                lessonId,
                connectionId = Context.ConnectionId,
                userId = UserId,
                userFullName = UserFullName,
                userRole = UserRole
            });
        }

        /// <summary>
        /// İstifadəçi ekran paylaşımını dayandırır.
        /// </summary>
        public async Task StopScreenShare(int lessonId)
        {
            if (!await _service.CanAccessClassroomAsync(lessonId, UserId))
                throw new HubException("Forbidden");

            // Yalnız paylaşan özü dayandıra bilər (və ya owner artıq dəyişib)
            if (_screenSharers.TryGetValue(lessonId, out var existing) &&
                existing.ConnectionId != Context.ConnectionId &&
                existing.UserId != UserId)
            {
                // Müəllim override: müəllim başqasının paylaşımını dayandıra bilər
                if (UserRole != "Teacher" && UserRole != "SuperAdmin")
                    return;
            }

            ClearScreenShareIfOwner(lessonId, Context.ConnectionId, notifyGroup: false, force: true);

            await Clients.Group(GroupName(lessonId)).SendAsync("UserStoppedScreenShare", new
            {
                lessonId,
                connectionId = Context.ConnectionId,
                userId = UserId,
                userFullName = UserFullName
            });
        }

        private static void ClearScreenShareIfOwner(int lessonId, string connectionId, bool notifyGroup, bool force = false)
        {
            if (_screenSharers.TryGetValue(lessonId, out var existing))
            {
                if (force || existing.ConnectionId == connectionId)
                    _screenSharers.TryRemove(lessonId, out _);
            }
        }

        // ── WebRTC signaling (target connection) ──

        public Task SendOffer(int lessonId, string targetConnectionId, object offer)
            => Clients.Client(targetConnectionId).SendAsync("ReceiveOffer",
                new { fromConnectionId = Context.ConnectionId, fromUserId = UserId, fromUserFullName = UserFullName, fromUserRole = UserRole, offer });

        public Task SendAnswer(int lessonId, string targetConnectionId, object answer)
            => Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer",
                new { fromConnectionId = Context.ConnectionId, fromUserId = UserId, fromUserFullName = UserFullName, fromUserRole = UserRole, answer });

        public Task SendIceCandidate(int lessonId, string targetConnectionId, object candidate)
            => Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate",
                new { fromConnectionId = Context.ConnectionId, candidate });

        // ── Müəllim idarəetməsi: tələbə mikrofonunu bağlamaq ──

        /// <summary>
        /// Müəllim konkret tələbənin mikrofonunu məcburi bağlayır.
        /// Yalnız dərsin sahib müəllimi icra edə bilər.
        /// </summary>
        public async Task MuteParticipant(int lessonId, string targetUserId)
        {
            if (!await _service.IsClassroomTeacherAsync(lessonId, UserId))
                throw new HubException("Forbidden");
            if (string.IsNullOrEmpty(targetUserId)) return;

            // Hədəf istifadəçinin bütün connection-larına məcburi mute komandası
            await Clients.User(targetUserId).SendAsync("ForceMutedByTeacher", new
            {
                lessonId,
                byUserFullName = UserFullName
            });

            // Bütün qrupa status yeniləmə (participant list badge üçün)
            await Clients.Group(GroupName(lessonId)).SendAsync("ParticipantMuted", new
            {
                lessonId,
                userId = targetUserId,
                muted = true,
                byUserFullName = UserFullName
            });
        }

        /// <summary>
        /// Müəllim bütün tələbələri mute edir (öz mikrofonuna təsir etmir).
        /// </summary>
        public async Task MuteAllParticipants(int lessonId)
        {
            if (!await _service.IsClassroomTeacherAsync(lessonId, UserId))
                throw new HubException("Forbidden");

            await Clients.GroupExcept(GroupName(lessonId), Context.ConnectionId)
                .SendAsync("ForceMutedByTeacher", new { lessonId, byUserFullName = UserFullName });

            await Clients.Group(GroupName(lessonId)).SendAsync("AllParticipantsMuted", new
            {
                lessonId,
                byUserFullName = UserFullName
            });
        }

        // ── Classroom chat ──
        public async Task SendClassMessage(int lessonId, string message)
        {
            if (!await _service.CanAccessClassroomAsync(lessonId, UserId))
                throw new HubException("Forbidden");

            var dto = await _service.SaveClassroomChatMessageAsync(lessonId, UserId, message ?? "");
            await Clients.Group(GroupName(lessonId)).SendAsync("ReceiveClassMessage", dto);
        }

        // ── Disconnect ──

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // SignalR group-dan avtomatik çıxarır; peer-lər RTCPeerConnection-u
            // timeout/onclose üzərindən bağlayacaq.
            // Əgər bu connection ekran paylaşırdısa, statusu təmizlə və qrupu məlumatlandır.
            foreach (var kv in _screenSharers)
            {
                if (kv.Value.ConnectionId == Context.ConnectionId)
                {
                    if (_screenSharers.TryRemove(kv.Key, out var removed))
                    {
                        await Clients.Group(GroupName(kv.Key)).SendAsync("UserStoppedScreenShare", new
                        {
                            lessonId = kv.Key,
                            connectionId = removed.ConnectionId,
                            userId = removed.UserId,
                            userFullName = removed.FullName
                        });
                    }
                    break;
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
