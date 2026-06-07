using Microsoft.Extensions.Configuration;
using SIS.BLL.Services.DTOs.Common;
using SIS.BLL.Services.DTOs.Enrollment;
using SIS.BLL.Services.DTOs.Public;
using SIS.BLL.Services.DTOs.Student;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using System.Text.RegularExpressions;
using Group = SIS.Entities.Models.Group;

namespace SIS.BLL.Services.Implementations
{
    /// <summary>
    /// Public saytdan gələn kurs müraciətlərini idarə edir.
    /// — submit: validation + save + audit + (best-effort) email
    /// — admin: status / note update + Student-ə konversiya (+ optional enroll)
    /// Mövcud Course / Group / Student / Enrollment servislərinin biznes məntiqini pozmur.
    /// </summary>
    public class CourseApplicationService : ICourseApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _audit;
        private readonly IEmailService _email;
        private readonly IStudentService _studentService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IConfiguration _configuration;

        public CourseApplicationService(
            IUnitOfWork unitOfWork,
            IAuditLogService audit,
            IEmailService email,
            IStudentService studentService,
            IEnrollmentService enrollmentService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _audit = audit;
            _email = email;
            _studentService = studentService;
            _enrollmentService = enrollmentService;
            _configuration = configuration;
        }

        // ---------- PUBLIC: create ----------
        public async Task<int> CreateAsync(CreateCourseApplicationDto dto)
        {
            // basic validation
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new ArgumentException("Ad Soyad boş ola bilməz.");
            if (string.IsNullOrWhiteSpace(dto.Email) || !IsValidEmail(dto.Email))
                throw new ArgumentException("Düzgün email yazın.");
            if (string.IsNullOrWhiteSpace(dto.Phone))
                throw new ArgumentException("Telefon boş ola bilməz.");

            // Course validation (optional CourseId)
            Course? course = null;
            if (dto.CourseId.HasValue)
            {
                course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId.Value);
                if (course == null || !course.IsActive)
                    throw new ArgumentException("Seçilmiş kurs aktiv deyil.");
            }

            // Group validation (optional GroupId — must belong to selected course if any)
            Group? group = null;
            if (dto.GroupId.HasValue)
            {
                group = await _unitOfWork.Groups.GetByIdAsync(dto.GroupId.Value);
                if (group == null)
                    throw new ArgumentException("Seçilmiş qrup tapılmadı.");
                if (dto.CourseId.HasValue && group.CourseId != dto.CourseId.Value)
                    throw new ArgumentException("Qrup seçilmiş kursa aid deyil.");
            }

            var entity = new CourseApplication
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                Phone = dto.Phone.Trim(),
                CourseId = dto.CourseId,
                GroupId = dto.GroupId,
                Message = string.IsNullOrWhiteSpace(dto.Message) ? null : dto.Message.Trim(),
                Status = CourseApplicationStatus.New,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CourseApplications.AddAsync(entity);
            await _unitOfWork.SaveAsync();

            // audit (don't include full message body)
            await SafeAudit("CreateCourseApplication", entity.Id,
                $"New public course application received from {entity.FullName}.");

            // best-effort emails (must NOT fail submission)
            _ = TrySendEmailsAsync(entity, course, group);

            return entity.Id;
        }

        private async Task TrySendEmailsAsync(CourseApplication a, Course? course, Group? group)
        {
            try
            {
                var courseName = course?.Name ?? "—";
                var groupName  = group?.Name ?? "—";

                // 1) Applicant confirmation
                var applicantHtml = $@"
<!DOCTYPE html><html><body style='font-family:Segoe UI,Arial,sans-serif;background:#F8FAFC;padding:24px;'>
  <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:14px;padding:28px;border:1px solid #E2E8F0;'>
    <h2 style='color:#4F46E5;margin:0 0 8px;'>Müraciətiniz qəbul edildi</h2>
    <p style='color:#0F172A;margin:0 0 14px;'>Salam <strong>{System.Net.WebUtility.HtmlEncode(a.FullName)}</strong>,</p>
    <p style='color:#475569;line-height:1.6;'>
      Acadex Akademiyasına göndərdiyiniz müraciət uğurla qeyd olundu.
      Komandamız tezliklə sizinlə əlaqə saxlayacaq.
    </p>
    <div style='background:#F8FAFC;border-radius:10px;padding:14px;margin:18px 0;font-size:14px;color:#0F172A;'>
      <div><strong>Kurs:</strong> {System.Net.WebUtility.HtmlEncode(courseName)}</div>
      <div><strong>Qrup:</strong> {System.Net.WebUtility.HtmlEncode(groupName)}</div>
    </div>
    <p style='color:#64748B;font-size:13px;'>Hörmətlə, Acadex Komandası.</p>
  </div>
</body></html>";
                await _email.SendAsync(a.Email, "Your Acadex Application Has Been Received", applicantHtml);
            }
            catch { /* email göndərmə xətası application-u poza bilməz */ }

            try
            {
                // 2) Admin notification — uses Email:NotifyAdmin or fallback Email:From
                var adminTo = _configuration["Email:NotifyAdmin"] ?? _configuration["Email:From"];
                if (!string.IsNullOrWhiteSpace(adminTo))
                {
                    var adminHtml = $@"
<!DOCTYPE html><html><body style='font-family:Segoe UI,Arial,sans-serif;background:#F8FAFC;padding:24px;'>
  <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:14px;padding:24px;border:1px solid #E2E8F0;'>
    <h2 style='color:#4F46E5;margin:0 0 8px;'>New Course Application</h2>
    <table style='width:100%;font-size:14px;color:#0F172A;border-collapse:collapse;'>
      <tr><td style='padding:6px 0;color:#64748B;'>Name</td><td><strong>{System.Net.WebUtility.HtmlEncode(a.FullName)}</strong></td></tr>
      <tr><td style='padding:6px 0;color:#64748B;'>Email</td><td>{System.Net.WebUtility.HtmlEncode(a.Email)}</td></tr>
      <tr><td style='padding:6px 0;color:#64748B;'>Phone</td><td>{System.Net.WebUtility.HtmlEncode(a.Phone)}</td></tr>
      <tr><td style='padding:6px 0;color:#64748B;'>Course</td><td>{System.Net.WebUtility.HtmlEncode(course?.Name ?? "—")}</td></tr>
      <tr><td style='padding:6px 0;color:#64748B;'>Group</td><td>{System.Net.WebUtility.HtmlEncode(group?.Name ?? "—")}</td></tr>
    </table>
    {(string.IsNullOrEmpty(a.Message) ? "" : $"<div style='margin-top:14px;padding:12px;background:#F8FAFC;border-radius:10px;color:#0F172A;font-size:13px;'><strong>Mesaj:</strong><br/>{System.Net.WebUtility.HtmlEncode(a.Message)}</div>")}
    <p style='color:#64748B;font-size:12px;margin-top:16px;'>Admin paneldən idarə edin: /Admin/Public/Applications/Details/{a.Id}</p>
  </div>
</body></html>";
                    await _email.SendAsync(adminTo, "New Course Application — Acadex", adminHtml);
                }
            }
            catch { /* email göndərmə xətası application-u poza bilməz */ }
        }

        // ---------- ADMIN: read ----------
        public async Task<PagedResult<CourseApplicationListDto>> GetPagedAsync(CourseApplicationFilterDto filter)
        {
            var (items, total) = await _unitOfWork.CourseApplications.QueryAsync(
                filter.Search, filter.Status, filter.CourseId,
                filter.FromDate, filter.ToDate, filter.Page, filter.PageSize);

            return new PagedResult<CourseApplicationListDto>
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = total,
                Items = items.Select(a => new CourseApplicationListDto
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Email = a.Email,
                    Phone = a.Phone,
                    CourseName = a.Course?.Name,
                    GroupName = a.Group?.Name,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    ConvertedStudentId = a.ConvertedStudentId
                }).ToList()
            };
        }

        public async Task<CourseApplicationDetailDto?> GetDetailAsync(int id)
        {
            var a = await _unitOfWork.CourseApplications.GetWithRelationsAsync(id);
            if (a == null) return null;
            return new CourseApplicationDetailDto
            {
                Id = a.Id,
                FullName = a.FullName,
                Email = a.Email,
                Phone = a.Phone,
                CourseId = a.CourseId,
                CourseName = a.Course?.Name,
                GroupId = a.GroupId,
                GroupName = a.Group?.Name,
                Status = a.Status,
                Message = a.Message,
                AdminNote = a.AdminNote,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                ConvertedStudentId = a.ConvertedStudentId,
                ConvertedStudentName = a.ConvertedStudent?.AppUser?.FullName,
                ConvertedStudentNumber = a.ConvertedStudent?.StudentNumber
            };
        }

        // ---------- ADMIN: actions ----------
        public async Task UpdateStatusAsync(int id, UpdateApplicationStatusDto dto)
        {
            var a = await _unitOfWork.CourseApplications.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Müraciət tapılmadı.");

            if (a.Status == dto.Status) return;
            // Don't allow direct "ConvertedToStudent" via status update — must go through convert flow
            if (dto.Status == CourseApplicationStatus.ConvertedToStudent && a.ConvertedStudentId == null)
                throw new InvalidOperationException("Status yalnız 'Create Student' əməliyyatı vasitəsilə ConvertedToStudent ola bilər.");

            var oldStatus = a.Status;
            a.Status = dto.Status;
            a.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CourseApplications.Update(a);
            await _unitOfWork.SaveAsync();

            await SafeAudit("UpdateApplicationStatus", a.Id,
                $"Application {a.Id} status changed from {oldStatus} to {dto.Status}.",
                oldValue: oldStatus.ToString(), newValue: dto.Status.ToString());
        }

        public async Task UpdateAdminNoteAsync(int id, UpdateApplicationNoteDto dto)
        {
            var a = await _unitOfWork.CourseApplications.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Müraciət tapılmadı.");

            a.AdminNote = string.IsNullOrWhiteSpace(dto.AdminNote) ? null : dto.AdminNote.Trim();
            a.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CourseApplications.Update(a);
            await _unitOfWork.SaveAsync();

            await SafeAudit("UpdateApplicationNote", a.Id,
                $"Admin note updated on application {a.Id}.");
        }

        public async Task<ConvertApplicationResultDto> ConvertToStudentAsync(int id, ConvertApplicationToStudentDto dto)
        {
            var a = await _unitOfWork.CourseApplications.GetWithRelationsAsync(id)
                ?? throw new KeyNotFoundException("Müraciət tapılmadı.");

            if (a.ConvertedStudentId.HasValue)
                throw new InvalidOperationException("Bu müraciət artıq tələbəyə çevrilib.");

            // Create student through existing StudentService (handles AppUser + student number generation + audit)
            await _studentService.CreateStudentAsync(new CreateStudentDto
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Password = dto.Password,
                PhoneNumber = dto.Phone,
                EnrollmentYear = dto.EnrollmentYear > 0 ? dto.EnrollmentYear : DateTime.UtcNow.Year
            });

            // Resolve newly created student by email (StudentService doesn't return Id)
            var created = (await _unitOfWork.Students.GetAllWithUsersAsync())
                .FirstOrDefault(s => s.AppUser != null
                    && string.Equals(s.AppUser.Email, dto.Email, StringComparison.OrdinalIgnoreCase));

            if (created == null)
                throw new InvalidOperationException("Tələbə yaradıldı, lakin tapıla bilmədi.");

            bool enrolled = false;
            string? enrollmentMessage = null;

            if (dto.EnrollToGroup && a.GroupId.HasValue)
            {
                try
                {
                    await _enrollmentService.EnrollAsync(new CreateEnrollmentDto
                    {
                        StudentId = created.Id,
                        GroupId = a.GroupId.Value
                    });
                    enrolled = true;
                    await SafeAudit("EnrollStudentFromApplication", a.Id,
                        $"Student {created.AppUser?.FullName} enrolled to group {a.Group?.Name} from application {a.Id}.");
                }
                catch (Exception ex)
                {
                    enrollmentMessage = ex.Message;
                }
            }

            // Mark application as converted
            a.ConvertedStudentId = created.Id;
            a.Status = CourseApplicationStatus.ConvertedToStudent;
            a.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CourseApplications.Update(a);
            await _unitOfWork.SaveAsync();

            await SafeAudit("ConvertApplicationToStudent", a.Id,
                $"Application {a.Id} converted to student {created.AppUser?.FullName}.",
                newValue: $"StudentId={created.Id}");

            return new ConvertApplicationResultDto
            {
                ApplicationId = a.Id,
                StudentId = created.Id,
                StudentNumber = created.StudentNumber,
                Enrolled = enrolled,
                EnrollmentMessage = enrollmentMessage
            };
        }

        public async Task<CourseApplicationStatsDto> GetStatsAsync()
        {
            var all = await _unitOfWork.CourseApplications.GetAllAsync();
            return new CourseApplicationStatsDto
            {
                TotalCount      = all.Count(),
                NewCount        = all.Count(a => a.Status == CourseApplicationStatus.New),
                ContactedCount  = all.Count(a => a.Status == CourseApplicationStatus.Contacted),
                AcceptedCount   = all.Count(a => a.Status == CourseApplicationStatus.Accepted),
                ConvertedCount  = all.Count(a => a.Status == CourseApplicationStatus.ConvertedToStudent)
            };
        }

        // ---------- helpers ----------
        private async Task SafeAudit(string action, int id, string description,
            string? oldValue = null, string? newValue = null)
        {
            try
            {
                await _audit.LogAsync(action, nameof(CourseApplication), id.ToString(),
                    description, oldValue, newValue);
            }
            catch { /* audit log xətası əsas əməliyyatı poza bilməz */ }
        }

        private static bool IsValidEmail(string email)
            => Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));
    }
}
