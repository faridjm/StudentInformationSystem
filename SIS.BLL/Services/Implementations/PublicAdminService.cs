using Microsoft.EntityFrameworkCore;
using SIS.BLL.Services.DTOs.PublicAdmin;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.Context;
using SIS.Entities.Enums;
using SIS.Entities.Models;

namespace SIS.BLL.Services.Implementations
{
    /// <summary>
    /// PublicAdmin panel servisi — public marketing kontentini idarə edir.
    /// Akademik servislərdən (Course, Teacher, Student, Enrollment...) heç birini dəyişmir,
    /// yalnız onlardan oxuyur və yeni Public* entity-ləri ilə işləyir.
    /// </summary>
    public class PublicAdminService : IPublicAdminService
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogService _audit;

        public PublicAdminService(AppDbContext db, IAuditLogService audit)
        {
            _db = db; _audit = audit;
        }

        // ========================= DASHBOARD =========================
        public async Task<PublicAdminStatsDto> GetStatsAsync()
        {
            return new PublicAdminStatsDto
            {
                CoursesTotal       = await _db.Courses.CountAsync(c => c.IsActive && !c.IsDeleted),
                CoursesPublished   = await _db.PublicCourseProfiles.CountAsync(p => p.IsVisible),
                TeachersTotal      = await _db.Teachers.CountAsync(t => t.AppUser != null && t.AppUser.IsActive),
                TeachersPublished  = await _db.PublicTeacherProfiles.CountAsync(p => p.IsVisible),
                FacilitiesCount    = await _db.PublicFacilities.CountAsync(),
                GraduatesCount     = await _db.PublicGraduateStories.CountAsync(),
                FaqCount           = await _db.PublicFaqs.CountAsync(),
                NewApplications    = await _db.CourseApplications.CountAsync(a => a.Status == CourseApplicationStatus.New),
                NewContactMessages = await _db.PublicContactMessages.CountAsync(m => m.Status == ContactMessageStatus.New)
            };
        }

        // ========================= COURSE PROFILES =========================
        public async Task<List<PublicCourseProfileRowDto>> GetCourseProfilesAsync()
        {
            var courses = await _db.Courses
                .Include(c => c.Department)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var profiles = await _db.PublicCourseProfiles
                .ToDictionaryAsync(p => p.CourseId, p => p);

            return courses.Select(c =>
            {
                profiles.TryGetValue(c.Id, out var p);
                return new PublicCourseProfileRowDto
                {
                    Id              = p?.Id ?? 0,
                    CourseId        = c.Id,
                    CourseName      = c.Name,
                    DepartmentName  = c.Department?.Name,
                    CourseIsActive  = c.IsActive,
                    PublicTitle     = p?.PublicTitle,
                    CoverImageUrl   = p?.CoverImageUrl,
                    IsVisible       = p?.IsVisible ?? false,
                    IsFeatured      = p?.IsFeatured ?? false,
                    SortOrder       = p?.SortOrder ?? 0
                };
            }).ToList();
        }

        public async Task<PublicCourseProfileEditDto?> GetCourseProfileAsync(int courseId)
        {
            var c = await _db.Courses.FirstOrDefaultAsync(x => x.Id == courseId && !x.IsDeleted);
            if (c == null) return null;
            var p = await _db.PublicCourseProfiles.FirstOrDefaultAsync(x => x.CourseId == courseId);

            return new PublicCourseProfileEditDto
            {
                CourseId = courseId,
                PublicTitle = p?.PublicTitle,
                PublicShortDescription = p?.PublicShortDescription,
                PublicDescription = p?.PublicDescription ?? c.Description,
                CoverImageUrl = p?.CoverImageUrl,
                WhatYouWillLearn = p?.WhatYouWillLearn,
                CareerOutcomes = p?.CareerOutcomes,
                IsVisible = p?.IsVisible ?? false,
                IsFeatured = p?.IsFeatured ?? false,
                SortOrder = p?.SortOrder ?? 0
            };
        }

        public async Task UpsertCourseProfileAsync(PublicCourseProfileEditDto dto)
        {
            var p = await _db.PublicCourseProfiles.FirstOrDefaultAsync(x => x.CourseId == dto.CourseId);
            bool created = p == null;
            if (created)
            {
                p = new PublicCourseProfile { CourseId = dto.CourseId, CreatedAt = DateTime.UtcNow };
                _db.PublicCourseProfiles.Add(p);
            }
            p!.PublicTitle = Trim(dto.PublicTitle, 200);
            p.PublicShortDescription = Trim(dto.PublicShortDescription, 500);
            p.PublicDescription = Trim(dto.PublicDescription, 4000);
            p.CoverImageUrl = Trim(dto.CoverImageUrl, 500);
            p.WhatYouWillLearn = Trim(dto.WhatYouWillLearn, 4000);
            p.CareerOutcomes = Trim(dto.CareerOutcomes, 4000);
            p.IsVisible = dto.IsVisible;
            p.IsFeatured = dto.IsFeatured;
            p.SortOrder = dto.SortOrder;
            p.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await SafeAudit(created ? "CreatePublicCourseProfile" : "UpdatePublicCourseProfile",
                nameof(PublicCourseProfile), p.Id.ToString(),
                $"Public course profile for course {dto.CourseId} saved (visible={dto.IsVisible}).");
        }

        public async Task ToggleCourseVisibilityAsync(int courseId)
        {
            var p = await _db.PublicCourseProfiles.FirstOrDefaultAsync(x => x.CourseId == courseId)
                ?? throw new KeyNotFoundException("Public profile yoxdur — əvvəl yaradın.");
            p.IsVisible = !p.IsVisible;
            p.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await SafeAudit("TogglePublicCourseVisibility", nameof(PublicCourseProfile),
                p.Id.ToString(), $"Visibility set to {p.IsVisible} for course {courseId}.");
        }

        // ========================= TEACHER PROFILES =========================
        public async Task<List<PublicTeacherProfileRowDto>> GetTeacherProfilesAsync()
        {
            var teachers = await _db.Teachers
                .Include(t => t.AppUser)
                .OrderBy(t => t.AppUser!.FullName)
                .ToListAsync();

            var profiles = await _db.PublicTeacherProfiles
                .ToDictionaryAsync(p => p.TeacherId, p => p);

            return teachers.Select(t =>
            {
                profiles.TryGetValue(t.Id, out var p);
                return new PublicTeacherProfileRowDto
                {
                    Id              = p?.Id ?? 0,
                    TeacherId       = t.Id,
                    TeacherName     = t.AppUser?.FullName ?? "",
                    TeacherEmail    = t.AppUser?.Email,
                    TeacherIsActive = t.AppUser?.IsActive ?? false,
                    DisplayName     = p?.DisplayName,
                    PublicTitle     = p?.PublicTitle,
                    PhotoUrl        = p?.PhotoUrl,
                    IsVisible       = p?.IsVisible ?? false,
                    IsFeatured      = p?.IsFeatured ?? false,
                    SortOrder       = p?.SortOrder ?? 0
                };
            }).ToList();
        }

        public async Task<PublicTeacherProfileEditDto?> GetTeacherProfileAsync(int teacherId)
        {
            var t = await _db.Teachers.Include(x => x.AppUser).FirstOrDefaultAsync(x => x.Id == teacherId);
            if (t == null) return null;
            var p = await _db.PublicTeacherProfiles.FirstOrDefaultAsync(x => x.TeacherId == teacherId);

            return new PublicTeacherProfileEditDto
            {
                TeacherId      = teacherId,
                DisplayName    = p?.DisplayName ?? t.AppUser?.FullName,
                PublicTitle    = p?.PublicTitle,
                Bio            = p?.Bio,
                Specialization = p?.Specialization,
                PhotoUrl       = p?.PhotoUrl,
                LinkedInUrl    = p?.LinkedInUrl,
                IsVisible      = p?.IsVisible ?? false,
                IsFeatured     = p?.IsFeatured ?? false,
                SortOrder      = p?.SortOrder ?? 0
            };
        }

        public async Task UpsertTeacherProfileAsync(PublicTeacherProfileEditDto dto)
        {
            var p = await _db.PublicTeacherProfiles.FirstOrDefaultAsync(x => x.TeacherId == dto.TeacherId);
            bool created = p == null;
            if (created)
            {
                p = new PublicTeacherProfile { TeacherId = dto.TeacherId, CreatedAt = DateTime.UtcNow };
                _db.PublicTeacherProfiles.Add(p);
            }
            p!.DisplayName    = Trim(dto.DisplayName, 150);
            p.PublicTitle    = Trim(dto.PublicTitle, 200);
            p.Bio            = Trim(dto.Bio, 2000);
            p.Specialization = Trim(dto.Specialization, 300);
            p.PhotoUrl       = Trim(dto.PhotoUrl, 500);
            p.LinkedInUrl    = Trim(dto.LinkedInUrl, 500);
            p.IsVisible      = dto.IsVisible;
            p.IsFeatured     = dto.IsFeatured;
            p.SortOrder      = dto.SortOrder;
            p.UpdatedAt      = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await SafeAudit(created ? "CreatePublicTeacherProfile" : "UpdatePublicTeacherProfile",
                nameof(PublicTeacherProfile), p.Id.ToString(),
                $"Public teacher profile for teacher {dto.TeacherId} saved (visible={dto.IsVisible}).");
        }

        public async Task ToggleTeacherVisibilityAsync(int teacherId)
        {
            var p = await _db.PublicTeacherProfiles.FirstOrDefaultAsync(x => x.TeacherId == teacherId)
                ?? throw new KeyNotFoundException("Public profile yoxdur — əvvəl yaradın.");
            p.IsVisible = !p.IsVisible;
            p.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await SafeAudit("TogglePublicTeacherVisibility", nameof(PublicTeacherProfile),
                p.Id.ToString(), $"Visibility set to {p.IsVisible} for teacher {teacherId}.");
        }

        // ========================= FACILITIES =========================
        public async Task<List<PublicFacilityDto>> GetFacilitiesAsync(bool onlyVisible = false)
        {
            var q = _db.PublicFacilities.AsQueryable();
            if (onlyVisible) q = q.Where(f => f.IsVisible);
            return await q.OrderBy(f => f.SortOrder).ThenBy(f => f.Id)
                .Select(f => new PublicFacilityDto
                {
                    Id = f.Id, Title = f.Title, Description = f.Description,
                    Type = f.Type, Capacity = f.Capacity, ImageUrl = f.ImageUrl,
                    IsVisible = f.IsVisible, SortOrder = f.SortOrder
                }).ToListAsync();
        }
        public async Task<PublicFacilityDto?> GetFacilityAsync(int id)
            => await _db.PublicFacilities.Where(f => f.Id == id)
                .Select(f => new PublicFacilityDto
                {
                    Id = f.Id, Title = f.Title, Description = f.Description,
                    Type = f.Type, Capacity = f.Capacity, ImageUrl = f.ImageUrl,
                    IsVisible = f.IsVisible, SortOrder = f.SortOrder
                }).FirstOrDefaultAsync();
        public async Task<int> CreateFacilityAsync(PublicFacilityDto dto)
        {
            var e = new PublicFacility
            {
                Title = (dto.Title ?? "").Trim(),
                Description = Trim(dto.Description, 1000),
                Type = Trim(dto.Type, 80),
                Capacity = dto.Capacity,
                ImageUrl = Trim(dto.ImageUrl, 500),
                IsVisible = dto.IsVisible,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow
            };
            _db.PublicFacilities.Add(e);
            await _db.SaveChangesAsync();
            await SafeAudit("CreatePublicFacility", nameof(PublicFacility), e.Id.ToString(),
                $"Facility '{e.Title}' created.");
            return e.Id;
        }
        public async Task UpdateFacilityAsync(PublicFacilityDto dto)
        {
            var e = await _db.PublicFacilities.FindAsync(dto.Id)
                ?? throw new KeyNotFoundException();
            e.Title = (dto.Title ?? "").Trim();
            e.Description = Trim(dto.Description, 1000);
            e.Type = Trim(dto.Type, 80);
            e.Capacity = dto.Capacity;
            e.ImageUrl = Trim(dto.ImageUrl, 500);
            e.IsVisible = dto.IsVisible;
            e.SortOrder = dto.SortOrder;
            e.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await SafeAudit("UpdatePublicFacility", nameof(PublicFacility), e.Id.ToString(),
                $"Facility '{e.Title}' updated.");
        }
        public async Task DeleteFacilityAsync(int id)
        {
            var e = await _db.PublicFacilities.FindAsync(id)
                ?? throw new KeyNotFoundException();
            e.IsDeleted = true; e.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await SafeAudit("DeletePublicFacility", nameof(PublicFacility), id.ToString(), "Facility soft-deleted.");
        }

        // ========================= GRADUATES =========================
        public async Task<List<PublicGraduateStoryDto>> GetGraduatesAsync(bool onlyVisible = false)
        {
            var q = _db.PublicGraduateStories.AsQueryable();
            if (onlyVisible) q = q.Where(g => g.IsVisible);
            return await q.OrderBy(g => g.SortOrder).ThenByDescending(g => g.Id)
                .Select(MapGraduate).ToListAsync();
        }
        public async Task<PublicGraduateStoryDto?> GetGraduateAsync(int id)
            => await _db.PublicGraduateStories.Where(g => g.Id == id).Select(MapGraduate).FirstOrDefaultAsync();
        public async Task<int> CreateGraduateAsync(PublicGraduateStoryDto dto)
        {
            var e = new PublicGraduateStory
            {
                FullName = (dto.FullName ?? "").Trim(),
                PhotoUrl = Trim(dto.PhotoUrl, 500),
                GraduatedCourse = Trim(dto.GraduatedCourse, 200),
                GraduationYear = dto.GraduationYear,
                CurrentPosition = Trim(dto.CurrentPosition, 200),
                Story = Trim(dto.Story, 2000),
                IsVisible = dto.IsVisible,
                IsFeatured = dto.IsFeatured,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow
            };
            _db.PublicGraduateStories.Add(e);
            await _db.SaveChangesAsync();
            await SafeAudit("CreateGraduateStory", nameof(PublicGraduateStory), e.Id.ToString(),
                $"Graduate '{e.FullName}' created.");
            return e.Id;
        }
        public async Task UpdateGraduateAsync(PublicGraduateStoryDto dto)
        {
            var e = await _db.PublicGraduateStories.FindAsync(dto.Id) ?? throw new KeyNotFoundException();
            e.FullName = (dto.FullName ?? "").Trim();
            e.PhotoUrl = Trim(dto.PhotoUrl, 500);
            e.GraduatedCourse = Trim(dto.GraduatedCourse, 200);
            e.GraduationYear = dto.GraduationYear;
            e.CurrentPosition = Trim(dto.CurrentPosition, 200);
            e.Story = Trim(dto.Story, 2000);
            e.IsVisible = dto.IsVisible;
            e.IsFeatured = dto.IsFeatured;
            e.SortOrder = dto.SortOrder;
            e.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await SafeAudit("UpdateGraduateStory", nameof(PublicGraduateStory), e.Id.ToString(),
                $"Graduate '{e.FullName}' updated.");
        }
        public async Task DeleteGraduateAsync(int id)
        {
            var e = await _db.PublicGraduateStories.FindAsync(id) ?? throw new KeyNotFoundException();
            e.IsDeleted = true; e.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await SafeAudit("DeleteGraduateStory", nameof(PublicGraduateStory), id.ToString(), "Soft-deleted.");
        }

        private static readonly System.Linq.Expressions.Expression<Func<PublicGraduateStory, PublicGraduateStoryDto>> MapGraduate =
            g => new PublicGraduateStoryDto
            {
                Id = g.Id, FullName = g.FullName, PhotoUrl = g.PhotoUrl,
                GraduatedCourse = g.GraduatedCourse, GraduationYear = g.GraduationYear,
                CurrentPosition = g.CurrentPosition, Story = g.Story,
                IsVisible = g.IsVisible, IsFeatured = g.IsFeatured, SortOrder = g.SortOrder
            };

        // ========================= FAQ =========================
        public async Task<List<PublicFaqDto>> GetFaqsAsync(bool onlyVisible = false)
        {
            var q = _db.PublicFaqs.AsQueryable();
            if (onlyVisible) q = q.Where(f => f.IsVisible);
            return await q.OrderBy(f => f.SortOrder).ThenBy(f => f.Id)
                .Select(f => new PublicFaqDto
                {
                    Id = f.Id, Question = f.Question, Answer = f.Answer,
                    Category = f.Category, IsVisible = f.IsVisible, SortOrder = f.SortOrder
                }).ToListAsync();
        }
        public async Task<PublicFaqDto?> GetFaqAsync(int id)
            => await _db.PublicFaqs.Where(f => f.Id == id)
                .Select(f => new PublicFaqDto
                {
                    Id = f.Id, Question = f.Question, Answer = f.Answer,
                    Category = f.Category, IsVisible = f.IsVisible, SortOrder = f.SortOrder
                }).FirstOrDefaultAsync();
        public async Task<int> CreateFaqAsync(PublicFaqDto dto)
        {
            var e = new PublicFaq
            {
                Question = (dto.Question ?? "").Trim(),
                Answer = (dto.Answer ?? "").Trim(),
                Category = Trim(dto.Category, 80),
                IsVisible = dto.IsVisible,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow
            };
            _db.PublicFaqs.Add(e);
            await _db.SaveChangesAsync();
            await SafeAudit("CreatePublicFaq", nameof(PublicFaq), e.Id.ToString(), "FAQ created.");
            return e.Id;
        }
        public async Task UpdateFaqAsync(PublicFaqDto dto)
        {
            var e = await _db.PublicFaqs.FindAsync(dto.Id) ?? throw new KeyNotFoundException();
            e.Question = (dto.Question ?? "").Trim();
            e.Answer = (dto.Answer ?? "").Trim();
            e.Category = Trim(dto.Category, 80);
            e.IsVisible = dto.IsVisible;
            e.SortOrder = dto.SortOrder;
            e.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await SafeAudit("UpdatePublicFaq", nameof(PublicFaq), e.Id.ToString(), "FAQ updated.");
        }
        public async Task DeleteFaqAsync(int id)
        {
            var e = await _db.PublicFaqs.FindAsync(id) ?? throw new KeyNotFoundException();
            e.IsDeleted = true; e.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await SafeAudit("DeletePublicFaq", nameof(PublicFaq), id.ToString(), "FAQ soft-deleted.");
        }

        // ========================= CONTACT MESSAGES =========================
        public async Task<int> CreateContactMessageAsync(CreatePublicContactMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName)) throw new ArgumentException("Ad Soyad boş ola bilməz.");
            if (string.IsNullOrWhiteSpace(dto.Email))    throw new ArgumentException("Email boş ola bilməz.");
            if (string.IsNullOrWhiteSpace(dto.Message))  throw new ArgumentException("Mesaj boş ola bilməz.");

            var e = new PublicContactMessage
            {
                FullName = dto.FullName.Trim(),
                Email    = dto.Email.Trim(),
                Phone    = Trim(dto.Phone, 40),
                Subject  = Trim(dto.Subject, 200),
                Message  = dto.Message.Trim(),
                Status   = ContactMessageStatus.New,
                CreatedAt = DateTime.UtcNow
            };
            _db.PublicContactMessages.Add(e);
            await _db.SaveChangesAsync();
            await SafeAudit("CreatePublicContactMessage", nameof(PublicContactMessage), e.Id.ToString(),
                $"New contact message from {e.FullName}.");
            return e.Id;
        }
        public async Task<List<PublicContactMessageListDto>> GetContactMessagesAsync(ContactMessageStatus? status = null)
        {
            var q = _db.PublicContactMessages.AsQueryable();
            if (status.HasValue) q = q.Where(m => m.Status == status.Value);
            return await q.OrderByDescending(m => m.CreatedAt)
                .Select(m => new PublicContactMessageListDto
                {
                    Id = m.Id, FullName = m.FullName, Email = m.Email, Phone = m.Phone,
                    Subject = m.Subject, Status = m.Status, CreatedAt = m.CreatedAt
                }).ToListAsync();
        }
        public async Task<PublicContactMessageDetailDto?> GetContactMessageAsync(int id)
        {
            var m = await _db.PublicContactMessages.FirstOrDefaultAsync(x => x.Id == id);
            if (m == null) return null;
            return new PublicContactMessageDetailDto
            {
                Id = m.Id, FullName = m.FullName, Email = m.Email, Phone = m.Phone,
                Subject = m.Subject, Status = m.Status, CreatedAt = m.CreatedAt,
                Message = m.Message, AdminNote = m.AdminNote, UpdatedAt = m.UpdatedAt
            };
        }
        public async Task UpdateContactStatusAsync(int id, UpdateContactStatusDto dto)
        {
            var m = await _db.PublicContactMessages.FindAsync(id) ?? throw new KeyNotFoundException();
            var old = m.Status;
            if (old == dto.Status) return;
            m.Status = dto.Status;
            m.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await SafeAudit("UpdateContactMessageStatus", nameof(PublicContactMessage), id.ToString(),
                $"Status {old} → {dto.Status}.", old.ToString(), dto.Status.ToString());
        }
        public async Task UpdateContactNoteAsync(int id, UpdateContactNoteDto dto)
        {
            var m = await _db.PublicContactMessages.FindAsync(id) ?? throw new KeyNotFoundException();
            m.AdminNote = Trim(dto.AdminNote, 2000);
            m.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await SafeAudit("UpdateContactMessageNote", nameof(PublicContactMessage), id.ToString(), "Admin note updated.");
        }

        // ========================= HELPERS =========================
        private static string? Trim(string? s, int max)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            return s.Length <= max ? s : s.Substring(0, max);
        }

        private async Task SafeAudit(string action, string entity, string? entityId, string desc,
            string? oldValue = null, string? newValue = null)
        {
            try { await _audit.LogAsync(action, entity, entityId, desc, oldValue, newValue); }
            catch { /* audit loglama xətası əsas əməliyyatı pozmamalıdır */ }
        }
    }
}
