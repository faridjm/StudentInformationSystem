using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIS.BLL.Services.DTOs.Archive;
using SIS.BLL.Services.DTOs.Common;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Identity;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class ArchiveService : IArchiveService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _audit;
        private readonly UserManager<AppUser> _userManager;

        public ArchiveService(IUnitOfWork unitOfWork, IAuditLogService audit, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _audit = audit;
            _userManager = userManager;
        }

        public async Task<PagedResult<ArchiveConversationListItemDto>> GetChatConversationsAsync(ArchiveChatFilterDto filter)
        {
            var (items, total) = await _unitOfWork.Archive.GetChatConversationsAsync(
                filter.Search, filter.Role, filter.Status,
                filter.FromDate, filter.ToDate?.Date.AddDays(1).AddTicks(-1),
                filter.GroupId, filter.Page, filter.PageSize);

            var ids = items.Select(c => c.Id).ToList();
            var lasts  = await _unitOfWork.Archive.GetLastMessagesAsync(ids);
            var counts = await _unitOfWork.Archive.GetMessageCountsAsync(ids);

            // Arxivləyən admin-in adı (UserId → FullName)
            var archivedByIds = items.Where(c => !string.IsNullOrEmpty(c.ArchivedByUserId))
                                     .Select(c => c.ArchivedByUserId!).Distinct().ToList();
            var archivedByMap = new Dictionary<string, string>();
            foreach (var uid in archivedByIds)
            {
                var u = await _userManager.FindByIdAsync(uid);
                if (u != null) archivedByMap[uid] = u.FullName;
            }

            var dtoItems = items.Select(c =>
            {
                lasts.TryGetValue(c.Id, out var last);
                counts.TryGetValue(c.Id, out var msgCount);
                return new ArchiveConversationListItemDto
                {
                    Id = c.Id,
                    GroupId = c.GroupId,
                    GroupName = c.Group?.Name ?? "",
                    CourseName = c.Group?.Course?.Name ?? "",
                    TeacherUserId = c.TeacherUserId,
                    TeacherFullName = c.TeacherUser?.FullName ?? "",
                    TeacherEmail = c.TeacherUser?.Email ?? "",
                    StudentUserId = c.StudentUserId,
                    StudentFullName = c.StudentUser?.FullName ?? "",
                    StudentEmail = c.StudentUser?.Email ?? "",
                    LastMessagePreview = TruncatePreview(last?.Content),
                    LastMessageAt = last?.SentAt,
                    MessageCount = msgCount,
                    UnreadCount = 0,
                    IsArchived = c.IsArchived,
                    ArchivedAt = c.ArchivedAt,
                    ArchivedByName = !string.IsNullOrEmpty(c.ArchivedByUserId) && archivedByMap.TryGetValue(c.ArchivedByUserId, out var n) ? n : null,
                    CreatedAt = c.CreatedAt
                };
            }).ToList();

            return new PagedResult<ArchiveConversationListItemDto>
            {
                Items = dtoItems,
                TotalCount = total,
                Page = filter.Page < 1 ? 1 : filter.Page,
                PageSize = filter.PageSize < 1 ? 25 : filter.PageSize
            };
        }

        public async Task<ArchiveConversationDetailDto?> GetChatConversationDetailAsync(int conversationId)
        {
            var c = await _unitOfWork.Archive.GetChatConversationDetailAsync(conversationId);
            if (c == null) return null;

            var messages = (await _unitOfWork.Archive.GetMessagesAsync(conversationId)).ToList();

            string? archivedByName = null;
            if (!string.IsNullOrEmpty(c.ArchivedByUserId))
            {
                var u = await _userManager.FindByIdAsync(c.ArchivedByUserId);
                archivedByName = u?.FullName;
            }

            var dto = new ArchiveConversationDetailDto
            {
                Id = c.Id,
                GroupId = c.GroupId,
                GroupName = c.Group?.Name ?? "",
                CourseName = c.Group?.Course?.Name ?? "",
                TeacherUserId = c.TeacherUserId,
                TeacherFullName = c.TeacherUser?.FullName ?? "",
                TeacherEmail = c.TeacherUser?.Email ?? "",
                StudentUserId = c.StudentUserId,
                StudentFullName = c.StudentUser?.FullName ?? "",
                StudentEmail = c.StudentUser?.Email ?? "",
                IsArchived = c.IsArchived,
                ArchivedAt = c.ArchivedAt,
                ArchivedByName = archivedByName,
                CreatedAt = c.CreatedAt,
                LastMessageAt = messages.LastOrDefault()?.SentAt,
                MessageCount = messages.Count,
                Messages = messages.Select(m => new ArchiveMessageDto
                {
                    Id = m.Id,
                    SenderUserId = m.SenderUserId,
                    SenderFullName = m.SenderUser?.FullName ?? "",
                    SenderRole = m.SenderUserId == c.TeacherUserId ? RoleConstants.Teacher : RoleConstants.Student,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead,
                    AttachmentType = m.AttachmentType,
                    AttachmentUrl = m.AttachmentUrl,
                    AttachmentFileName = m.AttachmentFileName,
                    AttachmentContentType = m.AttachmentContentType,
                    AttachmentSize = m.AttachmentSize,
                    DurationSeconds = m.DurationSeconds
                }).ToList()
            };
            return dto;
        }

        public async Task ArchiveConversationAsync(int conversationId, string superAdminId)
        {
            var c = await _unitOfWork.Archive.GetChatConversationDetailAsync(conversationId)
                ?? throw new NotFoundException("Söhbət tapılmadı");

            if (c.IsArchived) return; // idempotent

            c.IsArchived = true;
            c.ArchivedAt = DateTime.UtcNow;
            c.ArchivedByUserId = superAdminId;
            c.UpdatedAt = DateTime.UtcNow;

            // EF Core izləməsi
            _unitOfWork.Chat.GetType(); // keep reference (no-op)
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("ArchiveChatConversation", "ChatConversation", c.Id.ToString(),
                $"SuperAdmin söhbəti arxivləşdirdi: {c.TeacherUser?.FullName} ↔ {c.StudentUser?.FullName} (Group {c.Group?.Name})");
        }

        public async Task RestoreConversationAsync(int conversationId, string superAdminId)
        {
            var c = await _unitOfWork.Archive.GetChatConversationDetailAsync(conversationId)
                ?? throw new NotFoundException("Söhbət tapılmadı");

            if (!c.IsArchived) return; // idempotent

            c.IsArchived = false;
            c.ArchivedAt = null;
            c.ArchivedByUserId = null;
            c.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("RestoreChatConversation", "ChatConversation", c.Id.ToString(),
                $"SuperAdmin söhbəti bərpa etdi: {c.TeacherUser?.FullName} ↔ {c.StudentUser?.FullName} (Group {c.Group?.Name})");
        }

        // ── Helpers ──
        private static string TruncatePreview(string? content)
        {
            if (string.IsNullOrEmpty(content)) return "";
            const int max = 80;
            return content.Length <= max ? content : content.Substring(0, max) + "…";
        }
    }
}
