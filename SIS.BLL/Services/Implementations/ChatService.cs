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

        public ChatService(IUnitOfWork unitOfWork, INotificationService notifications)
        {
            _unitOfWork = unitOfWork;
            _notifications = notifications;
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
                    LastMessage = lastMsg?.Content,
                    LastMessageAt = lastMsg?.SentAt,
                    UnreadCount = unread
                });
            }
            return result;
        }

        public async Task<ConversationDto> GetOrCreateConversationAsync(string currentUserId, int groupId, string targetUserId)
        {
            // Kim teacher kim student — group-dan tapırıq
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

            // Reload with includes
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
            return messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderUserId = m.SenderUserId,
                SenderName = m.SenderUser?.FullName ?? "",
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead,
                IsMine = m.SenderUserId == currentUserId
            });
        }

        public async Task<MessageDto> SendMessageAsync(string senderUserId, int conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new BadRequestException("Mesaj boş ola bilməz");

            var conv = await _unitOfWork.Chat.GetConversationAsync(conversationId)
                ?? throw new NotFoundException("Söhbət tapılmadı");

            if (conv.TeacherUserId != senderUserId && conv.StudentUserId != senderUserId)
                throw new UnauthorizedException("Bu söhbətə giriş icazəniz yoxdur");

            var msg = await _unitOfWork.Chat.AddMessageAsync(new ChatMessage
            {
                ConversationId = conversationId,
                SenderUserId = senderUserId,
                Content = content.Trim()
            });

            // Qəbul edənə bildiriş
            var isSenderTeacher = conv.TeacherUserId == senderUserId;
            var receiverUserId = isSenderTeacher ? conv.StudentUserId : conv.TeacherUserId;
            var senderName = isSenderTeacher ? conv.TeacherUser?.FullName : conv.StudentUser?.FullName;
            var relatedUrl = isSenderTeacher ? "/Student/Messages" : "/Teacher/Messages";
            var snippet = content.Trim();
            if (snippet.Length > 80) snippet = snippet.Substring(0, 80) + "…";
            await _notifications.SendNotificationAsync(receiverUserId,
                $"Yeni mesaj: {senderName}", snippet, NotificationType.ChatMessage, relatedUrl);

            return new MessageDto
            {
                Id = msg.Id,
                ConversationId = conversationId,
                SenderUserId = senderUserId,
                SenderName = conv.TeacherUserId == senderUserId
                    ? conv.TeacherUser?.FullName ?? ""
                    : conv.StudentUser?.FullName ?? "",
                Content = msg.Content,
                SentAt = msg.SentAt,
                IsRead = false,
                IsMine = true
            };
        }

        public async Task MarkAsReadAsync(int conversationId, string userId)
            => await _unitOfWork.Chat.MarkAsReadAsync(conversationId, userId);

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _unitOfWork.Chat.GetUnreadCountAsync(userId);

        public async Task<bool> HasAccessAsync(int conversationId, string userId)
        {
            var conv = await _unitOfWork.Chat.GetConversationAsync(conversationId);
            return conv != null && (conv.TeacherUserId == userId || conv.StudentUserId == userId);
        }
    }
}
