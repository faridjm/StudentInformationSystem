using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SIS.BLL.Services.DTOs.Announcement;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Identity;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _audit;
        private readonly UserManager<AppUser> _userManager;
        private readonly IServiceScopeFactory _scopeFactory;

        public AnnouncementService(IUnitOfWork unitOfWork, IEmailService emailService,
            INotificationService notificationService, IAuditLogService audit,
            UserManager<AppUser> userManager, IServiceScopeFactory scopeFactory)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _notificationService = notificationService;
            _audit = audit;
            _userManager = userManager;
            _scopeFactory = scopeFactory;
        }

        // ── Listings ──
        public async Task<IEnumerable<AnnouncementDto>> GetAllAsync()
        {
            var items = await _unitOfWork.Announcements.GetAllWithRecipientsAsync();
            return items.Select(MapToDtoWithStats);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetGlobalAnnouncementsAsync()
        {
            var items = await _unitOfWork.Announcements.GetGlobalAnnouncementsAsync();
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetForStudentAsync(int studentId)
        {
            var items = await _unitOfWork.Announcements.GetForStudentAsync(studentId);
            return items.Select(MapToDto);
        }

        public Task<IEnumerable<AnnouncementDto>> GetAnnouncementsForStudentAsync(int studentId)
            => GetForStudentAsync(studentId);

        public async Task<IEnumerable<AnnouncementDto>> GetForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<AnnouncementDto>();
            var items = await _unitOfWork.Announcements.GetForUserAsync(userId);
            return items.Select(a =>
            {
                var dto = MapToDto(a);
                dto.IsRead = a.Recipients.Any(r => r.UserId == userId && r.IsRead);
                return dto;
            });
        }

        public async Task<AnnouncementDetailDto?> GetDetailAsync(int id, string? viewerUserId = null)
        {
            var a = await _unitOfWork.Announcements.GetWithRecipientsAsync(id);
            if (a == null) return null;

            var dto = new AnnouncementDetailDto
            {
                Id = a.Id, Title = a.Title, Content = a.Content,
                AuthorName = a.Author?.FullName ?? "",
                TargetType = a.TargetType, CourseName = a.Group?.Name,
                CreatedAt = a.CreatedAt, IsGlobal = a.IsGlobal,
                AttachmentUrl = a.AttachmentUrl,
                SendEmail = a.SendEmail, EmailSentAt = a.EmailSentAt,
                RecipientCount = a.Recipients.Count,
                ReadCount = a.Recipients.Count(r => r.IsRead),
                EmailSentCount = a.Recipients.Count(r => r.EmailSent),
                IsRead = !string.IsNullOrEmpty(viewerUserId)
                         && a.Recipients.Any(r => r.UserId == viewerUserId && r.IsRead)
            };

            // Hər recipient üçün role-u müəyyən edək (UserManager-dan).
            var recipientRows = new List<AnnouncementRecipientRowDto>();
            foreach (var r in a.Recipients.OrderByDescending(r => r.IsRead).ThenBy(r => r.User.FullName))
            {
                string? role = null;
                if (r.User != null)
                {
                    var roles = await _userManager.GetRolesAsync(r.User);
                    role = roles.FirstOrDefault();
                }
                recipientRows.Add(new AnnouncementRecipientRowDto
                {
                    UserId = r.UserId,
                    FullName = r.User?.FullName ?? "",
                    Email = r.User?.Email ?? "",
                    Role = role,
                    IsRead = r.IsRead, ReadAt = r.ReadAt,
                    EmailSent = r.EmailSent, EmailSentAt = r.EmailSentAt
                });
            }
            dto.Recipients = recipientRows;
            return dto;
        }

        public async Task MarkAsReadAsync(int announcementId, string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;
            var r = await _unitOfWork.AnnouncementRecipients.GetAsync(announcementId, userId);
            if (r == null || r.IsRead) return;
            r.IsRead = true;
            r.ReadAt = DateTime.UtcNow;
            _unitOfWork.AnnouncementRecipients.Update(r);
            await _unitOfWork.SaveAsync();
        }

        // ── Create ──
        public async Task CreateAnnouncementAsync(CreateAnnouncementDto dto, string authorId)
        {
            // 1. Validation
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new BadRequestException("Başlıq tələb olunur");
            if (string.IsNullOrWhiteSpace(dto.Content))
                throw new BadRequestException("Məzmun tələb olunur");

            if (dto.TargetType == AnnouncementTarget.GroupStudents && !dto.GroupId.HasValue)
                throw new BadRequestException("Qrup seçimi tələb olunur");
            if (dto.TargetType == AnnouncementTarget.SelectedUsers
                && (dto.UserIds == null || dto.UserIds.Count == 0))
                throw new BadRequestException("Ən az 1 istifadəçi seçilməlidir");

            // 2. Recipient resolution
            var recipientIds = await ResolveRecipientUserIdsAsync(dto);
            if (recipientIds.Count == 0)
                throw new BadRequestException("Seçilmiş hədəf üzrə istifadəçi tapılmadı.");

            // 3. Entity yarat
            var announcement = new Announcement
            {
                Title = dto.Title.Trim(),
                Content = dto.Content,
                TargetType = dto.TargetType,
                AuthorId = authorId,
                AttachmentUrl = dto.AttachmentUrl,
                IsGlobal = dto.TargetType == AnnouncementTarget.All,
                GroupId = dto.TargetType == AnnouncementTarget.GroupStudents ? dto.GroupId : null,
                SendEmail = dto.SendEmail,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Announcements.AddAsync(announcement);
            await _unitOfWork.SaveAsync();

            // 4. Recipient sətirləri
            var now = DateTime.UtcNow;
            foreach (var uid in recipientIds)
            {
                await _unitOfWork.AnnouncementRecipients.AddAsync(new AnnouncementRecipient
                {
                    AnnouncementId = announcement.Id,
                    UserId = uid,
                    IsRead = false,
                    EmailSent = false,
                    CreatedAt = now
                });
            }
            await _unitOfWork.SaveAsync();

            // 5. Notification (per recipient)
            try
            {
                await _notificationService.SendToManyAsync(
                    recipientIds,
                    "Yeni elan",
                    announcement.Title,
                    NotificationType.AnnouncementPosted,
                    relatedUrl: null);
            }
            catch { /* notification xətası elanı pozmasın */ }

            // 6. Email (background) — fərdi DI scope-da, request bitsə də işləyir.
            if (dto.SendEmail)
            {
                var annId = announcement.Id;
                _ = Task.Run(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var sp = scope.ServiceProvider;
                    var uow = sp.GetRequiredService<IUnitOfWork>();
                    var email = sp.GetRequiredService<IEmailService>();
                    var auditBg = sp.GetRequiredService<IAuditLogService>();
                    await SendAnnouncementEmailsBackgroundAsync(annId, uow, email, auditBg);
                });
            }

            // 7. Audit
            await _audit.LogAsync("Create", "Announcement", announcement.Id.ToString(),
                $"Admin elanı yaradıldı: '{announcement.Title}', target: {announcement.TargetType}, recipients: {recipientIds.Count}");
        }

        public async Task<IEnumerable<AnnouncementDto>> GetByGroupAsync(int groupId)
        {
            var items = await _unitOfWork.Announcements.GetByGroupAsync(groupId);
            return items.Select(MapToDto);
        }

        public async Task DeleteAnnouncementAsync(int id)
        {
            var a = await _unitOfWork.Announcements.GetByIdAsync(id)
                ?? throw new NotFoundException("Elan tapılmadı");
            _unitOfWork.Announcements.SoftDelete(a);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Delete", "Announcement", id.ToString(),
                $"Elan silindi: '{a.Title}'");
        }

        // ── User Search ──
        public async Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Enumerable.Empty<UserSearchResultDto>();

            var q = query.Trim().ToLower();
            var users = await _userManager.Users
                .Where(u => u.IsActive
                         && (u.FullName.ToLower().Contains(q)
                          || (u.Email != null && u.Email.ToLower().Contains(q))))
                .OrderBy(u => u.FullName)
                .Take(20)
                .ToListAsync();

            var results = new List<UserSearchResultDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                results.Add(new UserSearchResultDto
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    Role = roles.FirstOrDefault() ?? ""
                });
            }
            return results;
        }

        // ── Recipient Resolution ──
        private async Task<List<string>> ResolveRecipientUserIdsAsync(CreateAnnouncementDto dto)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);

            switch (dto.TargetType)
            {
                case AnnouncementTarget.All:
                {
                    var students = await _userManager.GetUsersInRoleAsync(RoleConstants.Student);
                    var teachers = await _userManager.GetUsersInRoleAsync(RoleConstants.Teacher);
                    foreach (var u in students.Where(u => u.IsActive)) ids.Add(u.Id);
                    foreach (var u in teachers.Where(u => u.IsActive)) ids.Add(u.Id);
                    break;
                }
                case AnnouncementTarget.Students:
                {
                    var students = await _userManager.GetUsersInRoleAsync(RoleConstants.Student);
                    foreach (var u in students.Where(u => u.IsActive)) ids.Add(u.Id);
                    break;
                }
                case AnnouncementTarget.Teachers:
                {
                    var teachers = await _userManager.GetUsersInRoleAsync(RoleConstants.Teacher);
                    foreach (var u in teachers.Where(u => u.IsActive)) ids.Add(u.Id);
                    break;
                }
                case AnnouncementTarget.GroupStudents:
                case AnnouncementTarget.Course:
                {
                    if (!dto.GroupId.HasValue) break;
                    var enrollments = await _unitOfWork.Enrollments.GetByGroupAsync(dto.GroupId.Value);
                    foreach (var e in enrollments.Where(e => !e.IsDeleted
                                                          && e.Status == EnrollmentStatus.Active))
                    {
                        var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(e.StudentId);
                        if (student?.AppUser != null && student.AppUser.IsActive)
                            ids.Add(student.AppUserId);
                    }
                    break;
                }
                case AnnouncementTarget.SelectedUsers:
                {
                    if (dto.UserIds == null) break;
                    foreach (var uid in dto.UserIds.Distinct())
                    {
                        var user = await _userManager.FindByIdAsync(uid);
                        if (user != null && user.IsActive)
                            ids.Add(uid);
                    }
                    break;
                }
            }

            return ids.ToList();
        }

        // ── Email (fon scope-da işləyir) ──
        private static async Task SendAnnouncementEmailsBackgroundAsync(
            int announcementId, IUnitOfWork uow, IEmailService email, IAuditLogService audit)
        {
            try
            {
                var announcement = await uow.Announcements.GetWithRecipientsAsync(announcementId);
                if (announcement == null) return;

                int sentCount = 0;
                var now = DateTime.UtcNow;

                foreach (var r in announcement.Recipients)
                {
                    if (r.User == null || string.IsNullOrEmpty(r.User.Email)) continue;
                    try
                    {
                        await email.SendAnnouncementAsync(r.User.Email, r.User.FullName,
                            announcement.Title, announcement.Content);
                        r.EmailSent = true;
                        r.EmailSentAt = now;
                        uow.AnnouncementRecipients.Update(r);
                        sentCount++;
                    }
                    catch
                    {
                        // hər recipient üçün ayrıca xəta — bütün axını pozmuruq
                        r.EmailSent = false;
                    }
                }

                if (sentCount > 0)
                {
                    announcement.EmailSentAt = now;
                    uow.Announcements.Update(announcement);
                }
                await uow.SaveAsync();

                try
                {
                    await audit.LogAsync("SendEmail", "Announcement", announcement.Id.ToString(),
                        $"Email göndərildi: {sentCount}/{announcement.Recipients.Count}");
                }
                catch { }
            }
            catch
            {
                // Fon prosesi heç vaxt unhandled exception ilə bitməsin.
            }
        }

        // ── Mappers ──
        private static AnnouncementDto MapToDto(Announcement a) => new()
        {
            Id = a.Id, Title = a.Title, Content = a.Content,
            AuthorName = a.Author?.FullName ?? "",
            TargetType = a.TargetType, CreatedAt = a.CreatedAt,
            IsGlobal = a.IsGlobal, AttachmentUrl = a.AttachmentUrl,
            SendEmail = a.SendEmail, EmailSentAt = a.EmailSentAt,
            CourseName = a.Group?.Name
        };

        private static AnnouncementDto MapToDtoWithStats(Announcement a)
        {
            var dto = MapToDto(a);
            dto.RecipientCount = a.Recipients.Count;
            dto.ReadCount = a.Recipients.Count(r => r.IsRead);
            dto.EmailSentCount = a.Recipients.Count(r => r.EmailSent);
            return dto;
        }
    }
}
