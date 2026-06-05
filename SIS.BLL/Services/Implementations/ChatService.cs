using SIS.BLL.Services.DTOs.Chat;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notifications;
        private readonly IChatRealtimeNotifier _realtime;

        public ChatService(IUnitOfWork unitOfWork, INotificationService notifications,
            IChatRealtimeNotifier realtime)
        {
            _unitOfWork = unitOfWork;
            _notifications = notifications;
            _realtime = realtime;
        }

        public async Task<IEnumerable<ConversationDto>> GetMyConversationsAsync(string userId)
        {
            var conversations = await _unitOfWork.Chat.GetConversationsByUserAsync(userId);
            var result = new List<ConversationDto>();

            foreach (var c in conversations)
            {
                var isTeacher = c.TeacherUserId == userId;
                var otherUser = isTeacher ? c.StudentUser : c.TeacherUser;
                var lastMsg = c.Messages.FirstOrDefault();
                var unread = await _unitOfWork.Chat.GetUnreadCountForConversationAsync(c.Id, userId);

                result.Add(new ConversationDto
                {
                    Id = c.Id,
                    GroupId = c.GroupId,
                    GroupName = c.Group?.Name ?? "",
                    CourseName = c.Group?.Course?.Name ?? "",
                    OtherUserName = otherUser?.FullName ?? "",
                    OtherUserId = isTeacher ? c.StudentUserId : c.TeacherUserId,
                    LastMessage = BuildPreview(lastMsg),
                    LastMessageAt = lastMsg?.SentAt,
                    UnreadCount = unread
                });
            }
            return result;
        }

        public async Task<ConversationDto> GetOrCreateConversationAsync(string currentUserId, int groupId, string targetUserId)
        {
            var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(groupId)
                ?? throw new NotFoundException("Qrup tapılmadı");

            var teacherAppUserId = group.Teacher?.AppUserId
                ?? throw new NotFoundException("Müəllim tapılmadı");

            string teacherUserId, studentUserId;
            if (currentUserId == teacherAppUserId)
            {
                teacherUserId = currentUserId;
                studentUserId = targetUserId;
            }
            else
            {
                teacherUserId = targetUserId;
                studentUserId = currentUserId;
            }

            var existing = await _unitOfWork.Chat.FindConversationAsync(groupId, teacherUserId, studentUserId);
            if (existing != null)
            {
                var unread = await _unitOfWork.Chat.GetUnreadCountForConversationAsync(existing.Id, currentUserId);
                var isTeacher = existing.TeacherUserId == currentUserId;
                var otherUser = isTeacher ? existing.StudentUser : existing.TeacherUser;
                return new ConversationDto
                {
                    Id = existing.Id, GroupId = groupId,
                    GroupName = existing.Group?.Name ?? "",
                    CourseName = existing.Group?.Course?.Name ?? "",
                    OtherUserName = otherUser?.FullName ?? "",
                    OtherUserId = isTeacher ? existing.StudentUserId : existing.TeacherUserId,
                    UnreadCount = unread
                };
            }

            var conv = await _unitOfWork.Chat.CreateConversationAsync(new ChatConversation
            {
                GroupId = groupId,
                TeacherUserId = teacherUserId,
                StudentUserId = studentUserId
            });

            conv = await _unitOfWork.Chat.GetConversationAsync(conv.Id);
            var isT = conv!.TeacherUserId == currentUserId;
            var other = isT ? conv.StudentUser : conv.TeacherUser;
            return new ConversationDto
            {
                Id = conv.Id, GroupId = groupId,
                GroupName = conv.Group?.Name ?? "",
                CourseName = conv.Group?.Course?.Name ?? "",
                OtherUserName = other?.FullName ?? "",
                OtherUserId = isT ? conv.StudentUserId : conv.TeacherUserId
            };
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, string currentUserId, int take = 50, int skip = 0)
        {
            var messages = await _unitOfWork.Chat.GetMessagesAsync(conversationId, take, skip);
            return messages.Select(m => MapMessage(m, currentUserId));
        }

        public Task<MessageDto> SendMessageAsync(string senderUserId, int conversationId, string content)
            => SendInternalAsync(senderUserId, conversationId, content, ChatAttachmentType.None,
                                 null, null, null, null, null);

        public Task<MessageDto> SendAttachmentMessageAsync(string senderUserId, SendAttachmentMessageDto dto)
            => SendInternalAsync(senderUserId, dto.ConversationId, dto.Content,
                                 dto.AttachmentType,
                                 dto.AttachmentUrl, dto.AttachmentFileName,
                                 dto.AttachmentContentType, dto.AttachmentSize, dto.DurationSeconds);

        private async Task<MessageDto> SendInternalAsync(
            string senderUserId, int conversationId, string? content,
            ChatAttachmentType attachmentType,
            string? attachmentUrl, string? attachmentFileName,
            string? attachmentContentType, long? attachmentSize, int? durationSeconds)
        {
            var hasAttachment = attachmentType != ChatAttachmentType.None;
            var hasText = !string.IsNullOrWhiteSpace(content);
            if (!hasText && !hasAttachment)
                throw new BadRequestException("Mesaj boş ola bilməz");

            var conv = await _unitOfWork.Chat.GetConversationAsync(conversationId)
                ?? throw new NotFoundException("Söhbət tapılmadı");

            if (conv.TeacherUserId != senderUserId && conv.StudentUserId != senderUserId)
                throw new UnauthorizedException("Bu söhbətə giriş icazəniz yoxdur");

            var msg = await _unitOfWork.Chat.AddMessageAsync(new ChatMessage
            {
                ConversationId = conversationId,
                SenderUserId = senderUserId,
                // DB-də Content NOT NULL ola bilər (köhnə schema). Migration tətbiq olunduqdan sonra
                // bu xətt boş string yerinə null saxlaya bilər. Hazırda təhlükəsiz fallback:
                Content = hasText ? content!.Trim() : "",
                AttachmentType = attachmentType,
                AttachmentUrl = attachmentUrl,
                AttachmentFileName = attachmentFileName,
                AttachmentContentType = attachmentContentType,
                AttachmentSize = attachmentSize,
                DurationSeconds = durationSeconds
            });

            // SenderUser navigation-ı üçün təkrar oxu
            var senderUser = (conv.TeacherUserId == senderUserId)
                ? conv.TeacherUser
                : conv.StudentUser;

            var dto = new MessageDto
            {
                Id = msg.Id,
                ConversationId = conversationId,
                SenderUserId = senderUserId,
                SenderName = senderUser?.FullName ?? "",
                Content = msg.Content,
                SentAt = msg.SentAt,
                IsRead = false,
                IsMine = false, // realtime alıcılar üçün — frontend SenderUserId ilə müqayisə edir
                AttachmentType = msg.AttachmentType,
                AttachmentUrl = msg.AttachmentUrl,
                AttachmentFileName = msg.AttachmentFileName,
                AttachmentContentType = msg.AttachmentContentType,
                AttachmentSize = msg.AttachmentSize,
                DurationSeconds = msg.DurationSeconds
            };

            // ── Realtime push (group fan-out, həm sender həm receiver eyni payload alır) ──
            try { await _realtime.NotifyMessageAsync(conversationId, dto); }
            catch { /* realtime xətası mesajın saxlanmasını pozmasın */ }

            // ── Notification (yalnız qarşı tərəfə) ──
            var isSenderTeacher = conv.TeacherUserId == senderUserId;
            var receiverUserId = isSenderTeacher ? conv.StudentUserId : conv.TeacherUserId;
            var senderName = senderUser?.FullName ?? "";
            var relatedUrl = isSenderTeacher ? "/Student/Messages" : "/Teacher/Messages";

            string title = attachmentType switch
            {
                ChatAttachmentType.Image => $"Yeni şəkil: {senderName}",
                ChatAttachmentType.File  => $"Yeni fayl: {senderName}",
                ChatAttachmentType.Voice => $"Yeni səsli mesaj: {senderName}",
                _ => $"Yeni mesaj: {senderName}"
            };
            var snippet = BuildPreview(msg);
            try
            {
                await _notifications.SendNotificationAsync(receiverUserId, title, snippet,
                    NotificationType.ChatMessage, relatedUrl);
            }
            catch { }

            // Sender üçün IsMine = true olan dto qaytaraq (REST cavabında)
            dto.IsMine = true;
            return dto;
        }

        public async Task MarkAsReadAsync(int conversationId, string userId)
        {
            await _unitOfWork.Chat.MarkAsReadAsync(conversationId, userId);
            try { await _realtime.NotifyMessagesReadAsync(conversationId, userId); }
            catch { }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _unitOfWork.Chat.GetUnreadCountAsync(userId);

        public async Task<bool> HasAccessAsync(int conversationId, string userId)
        {
            var conv = await _unitOfWork.Chat.GetConversationAsync(conversationId);
            return conv != null && (conv.TeacherUserId == userId || conv.StudentUserId == userId);
        }

        // ── Helpers ──
        private static MessageDto MapMessage(ChatMessage m, string currentUserId) => new()
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            SenderUserId = m.SenderUserId,
            SenderName = m.SenderUser?.FullName ?? "",
            Content = m.Content,
            SentAt = m.SentAt,
            IsRead = m.IsRead,
            IsMine = m.SenderUserId == currentUserId,
            AttachmentType = m.AttachmentType,
            AttachmentUrl = m.AttachmentUrl,
            AttachmentFileName = m.AttachmentFileName,
            AttachmentContentType = m.AttachmentContentType,
            AttachmentSize = m.AttachmentSize,
            DurationSeconds = m.DurationSeconds
        };

        private static string? BuildPreview(ChatMessage? m)
        {
            if (m == null) return null;
            return m.AttachmentType switch
            {
                ChatAttachmentType.Image => "📷 Şəkil",
                ChatAttachmentType.File  => $"📎 {m.AttachmentFileName ?? "Fayl"}",
                ChatAttachmentType.Voice => $"🎤 Səsli mesaj",
                _ => m.Content
            };
        }
    }
}
