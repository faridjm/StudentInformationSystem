using SIS.BLL.Services.DTOs.OnlineLesson;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Time;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class OnlineLessonService : IOnlineLessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _audit;
        private readonly INotificationService _notifications;
        private readonly IAttendanceService _attendance;
        private readonly IOnlineClassNotifier _classNotifier;
        private readonly IClock _clock;

        public OnlineLessonService(IUnitOfWork unitOfWork, IAuditLogService audit,
            INotificationService notifications, IAttendanceService attendance,
            IOnlineClassNotifier classNotifier, IClock clock)
        {
            _unitOfWork = unitOfWork;
            _audit = audit;
            _notifications = notifications;
            _attendance = attendance;
            _classNotifier = classNotifier;
            _clock = clock;
        }

        // ── Yaratma / Yeniləmə / Lifecycle ──

        public async Task<int> CreateOnlineLessonAsync(CreateOnlineLessonDto dto, int teacherId)
        {
            var session = await _unitOfWork.ClassSessions.GetByIdAsync(dto.ClassSessionId)
                ?? throw new NotFoundException("Dərs sessiyası tapılmadı");

            var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(session.GroupId)
                ?? throw new NotFoundException("Qrup tapılmadı");

            // Yalnız qrupun müəllimi yarada bilər
            if (group.TeacherId != teacherId)
                throw new UnauthorizedException("Bu qrup üçün online dərs yaratmaq icazəniz yoxdur");

            // Bir sessiyada eyni anda yalnız bir aktiv (Scheduled/Live) dərs ola bilər
            var active = await _unitOfWork.OnlineLessons.GetActiveBySessionAsync(dto.ClassSessionId);
            if (active != null)
                throw new BadRequestException($"Bu sessiya üçün artıq aktiv online dərs var (#{active.Id})");

            // Daxili classroom üçün link/platforma vacib deyil — opsional saxlanılır.
            // Verilməyibsə qrup default-ları götürülür (köhnə uyğunluq).
            var meetingUrl = string.IsNullOrWhiteSpace(dto.MeetingUrl) ? group.DefaultMeetingUrl : dto.MeetingUrl;
            var platform = dto.Platform ?? group.DefaultOnlinePlatform;

            var title = string.IsNullOrWhiteSpace(dto.Title)
                ? BuildDefaultTitle(group.Name, session.SessionNumber)
                : (dto.Title!.Length > 200 ? dto.Title.Substring(0, 200) : dto.Title.Trim());

            var lesson = new OnlineLesson
            {
                GroupId = group.Id,
                ClassSessionId = session.Id,
                TeacherId = teacherId,
                Title = title,
                Description = dto.Description,
                MeetingUrl = meetingUrl,
                Platform = platform,
                Status = OnlineLessonStatus.Scheduled,
                IsActive = true,
                CreatedAt = _clock.UtcNow
            };

            await _unitOfWork.OnlineLessons.AddAsync(lesson);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("CreateOnlineLesson", "OnlineLesson", lesson.Id.ToString(),
                $"Online dərs yaradıldı: {lesson.Title}");

            return lesson.Id;
        }

        public async Task UpdateOnlineLessonAsync(UpdateOnlineLessonDto dto, int teacherId)
        {
            var lesson = await _unitOfWork.OnlineLessons.GetByIdAsync(dto.Id)
                ?? throw new NotFoundException("Online dərs tapılmadı");

            EnsureOwningTeacher(lesson, teacherId);

            if (lesson.Status == OnlineLessonStatus.Ended || lesson.Status == OnlineLessonStatus.Cancelled)
                throw new BadRequestException("Bitmiş və ya ləğv olunmuş dərsi yeniləmək olmaz");

            // Daxili classroom üçün link/platforma vacib deyil — verilibsə saxlanılır.
            lesson.Title = string.IsNullOrWhiteSpace(dto.Title)
                ? lesson.Title
                : (dto.Title.Length > 200 ? dto.Title.Substring(0, 200) : dto.Title.Trim());
            lesson.Description = dto.Description;
            if (!string.IsNullOrWhiteSpace(dto.MeetingUrl)) lesson.MeetingUrl = dto.MeetingUrl;
            if (dto.Platform.HasValue) lesson.Platform = dto.Platform;
            lesson.UpdatedAt = _clock.UtcNow;

            _unitOfWork.OnlineLessons.Update(lesson);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("UpdateOnlineLesson", "OnlineLesson", lesson.Id.ToString(),
                $"Online dərs yeniləndi: {lesson.Title}");
        }

        public async Task<StartOnlineLessonResultDto> StartOnlineLessonAsync(int classSessionId, int teacherId)
        {
            var session = await _unitOfWork.ClassSessions.GetByIdAsync(classSessionId)
                ?? throw new NotFoundException("Dərs sessiyası tapılmadı");

            var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(session.GroupId)
                ?? throw new NotFoundException("Qrup tapılmadı");

            if (group.TeacherId != teacherId)
                throw new UnauthorizedException("Bu qrup üçün online dərsə başlamaq icazəniz yoxdur");

            var lesson = await _unitOfWork.OnlineLessons.GetActiveBySessionAsync(classSessionId);

            if (lesson == null)
            {
                // Aktiv dərs yoxdursa daxili classroom üçün avtomatik yarat — link tələb olunmur.
                lesson = new OnlineLesson
                {
                    GroupId = group.Id,
                    ClassSessionId = session.Id,
                    TeacherId = teacherId,
                    Title = BuildDefaultTitle(group.Name, session.SessionNumber),
                    MeetingUrl = group.DefaultMeetingUrl,
                    Platform = group.DefaultOnlinePlatform,
                    Status = OnlineLessonStatus.Live,
                    StartedAt = _clock.UtcNow,
                    IsActive = true,
                    CreatedAt = _clock.UtcNow
                };
                await _unitOfWork.OnlineLessons.AddAsync(lesson);
            }
            else
            {
                if (lesson.Status == OnlineLessonStatus.Live)
                    throw new BadRequestException("Bu sessiyada artıq canlı online dərs var");

                lesson.Status = OnlineLessonStatus.Live;
                lesson.StartedAt = _clock.UtcNow;
                lesson.UpdatedAt = _clock.UtcNow;
                _unitOfWork.OnlineLessons.Update(lesson);
            }

            await _unitOfWork.SaveAsync();

            var classroomUrl = $"/Teacher/OnlineLessons/Classroom/{lesson.Id}";
            var studentClassroomUrl = $"/Student/OnlineLessons/Classroom/{lesson.Id}";

            // Qrupun tələbələrinə bildiriş — RelatedUrl tələbə classroom-una yönləndirir
            try
            {
                var userIds = await GetEnrolledStudentUserIdsAsync(group.Id);
                if (userIds.Count > 0)
                    await _notifications.SendToManyAsync(userIds,
                        "Online lesson started",
                        $"{group.Name} üçün online dərs başladı",
                        NotificationType.OnlineLessonStarted,
                        studentClassroomUrl);
            }
            catch { /* bildiriş xətası dərsi pozmasın */ }

            await _audit.LogAsync("StartOnlineLesson", "OnlineLesson", lesson.Id.ToString(),
                $"Online dərs başladıldı: {lesson.Title}");

            return new StartOnlineLessonResultDto
            {
                LessonId = lesson.Id,
                ClassroomUrl = classroomUrl
            };
        }

        public async Task EndOnlineLessonAsync(int onlineLessonId, int teacherId)
        {
            var lesson = await _unitOfWork.OnlineLessons.GetByIdAsync(onlineLessonId)
                ?? throw new NotFoundException("Online dərs tapılmadı");

            EnsureOwningTeacher(lesson, teacherId);

            if (lesson.Status != OnlineLessonStatus.Live)
                throw new BadRequestException("Yalnız canlı (Live) dərsi bitirmək olar");

            lesson.Status = OnlineLessonStatus.Ended;
            lesson.EndedAt = _clock.UtcNow;
            lesson.UpdatedAt = _clock.UtcNow;
            _unitOfWork.OnlineLessons.Update(lesson);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("EndOnlineLesson", "OnlineLesson", lesson.Id.ToString(),
                $"Online dərs bitirildi: {lesson.Title}");

            // Classroom-da olan peer-lərə dərs bitdi event-i göndər
            try { await _classNotifier.NotifyLessonEndedAsync(lesson.Id); } catch { }
        }

        public async Task CancelOnlineLessonAsync(int onlineLessonId, int teacherId)
        {
            var lesson = await _unitOfWork.OnlineLessons.GetByIdAsync(onlineLessonId)
                ?? throw new NotFoundException("Online dərs tapılmadı");

            EnsureOwningTeacher(lesson, teacherId);

            if (lesson.Status == OnlineLessonStatus.Ended || lesson.Status == OnlineLessonStatus.Cancelled)
                throw new BadRequestException("Bitmiş və ya artıq ləğv olunmuş dərsi ləğv etmək olmaz");

            lesson.Status = OnlineLessonStatus.Cancelled;
            lesson.UpdatedAt = _clock.UtcNow;
            _unitOfWork.OnlineLessons.Update(lesson);
            await _unitOfWork.SaveAsync();

            // Qrupun tələbələrinə bildiriş
            try
            {
                var group = await _unitOfWork.Groups.GetByIdAsync(lesson.GroupId);
                var userIds = await GetEnrolledStudentUserIdsAsync(lesson.GroupId);
                if (userIds.Count > 0)
                    await _notifications.SendToManyAsync(userIds,
                        "Online lesson cancelled",
                        $"{group?.Name} üçün online dərs ləğv edildi",
                        NotificationType.OnlineLessonCancelled,
                        "/Student/Courses");
            }
            catch { /* bildiriş xətası dərsi pozmasın */ }

            await _audit.LogAsync("CancelOnlineLesson", "OnlineLesson", lesson.Id.ToString(),
                $"Online dərs ləğv edildi: {lesson.Title}");

            // Classroom-da olan peer-lərə dərs ləğv olundu event-i göndər
            try { await _classNotifier.NotifyLessonCancelledAsync(lesson.Id); } catch { }
        }

        // ── Tələbə: qoşulma ──

        public async Task<JoinOnlineLessonResultDto> JoinOnlineLessonAsync(int onlineLessonId, int studentId)
        {
            var lesson = await _unitOfWork.OnlineLessons.GetByIdAsync(onlineLessonId)
                ?? throw new NotFoundException("Online dərs tapılmadı");

            // Tələbə bu qrupun aktiv üzvü olmalıdır
            var enrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(studentId, lesson.GroupId);
            if (!enrolled)
                throw new UnauthorizedException("Bu dərsə qoşulmaq icazəniz yoxdur");

            switch (lesson.Status)
            {
                case OnlineLessonStatus.Scheduled:
                    return new JoinOnlineLessonResultDto
                    {
                        CanJoin = false, Status = lesson.Status, Message = "Not started yet"
                    };
                case OnlineLessonStatus.Ended:
                    return new JoinOnlineLessonResultDto
                    {
                        CanJoin = false, Status = lesson.Status, Message = "Lesson ended"
                    };
                case OnlineLessonStatus.Cancelled:
                    return new JoinOnlineLessonResultDto
                    {
                        CanJoin = false, Status = lesson.Status, Message = "Lesson cancelled"
                    };
            }

            // Live — iştirakçını upsert et
            try
            {
                var participant = await _unitOfWork.OnlineLessons.GetParticipantAsync(onlineLessonId, studentId);
                if (participant == null)
                {
                    await _unitOfWork.OnlineLessons.AddParticipantAsync(new OnlineLessonParticipant
                    {
                        OnlineLessonId = onlineLessonId,
                        StudentId = studentId,
                        JoinedAt = _clock.UtcNow,
                        CreatedAt = _clock.UtcNow
                    });
                }
                else
                {
                    participant.JoinedAt = _clock.UtcNow;
                    _unitOfWork.OnlineLessons.UpdateParticipant(participant);
                }
                await _unitOfWork.SaveAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                // Eyni anda iki qoşulma cəhdi — unique index pozulsa, mövcud yazını yenilə
                var existing = await _unitOfWork.OnlineLessons.GetParticipantAsync(onlineLessonId, studentId);
                if (existing != null)
                {
                    existing.JoinedAt = _clock.UtcNow;
                    _unitOfWork.OnlineLessons.UpdateParticipant(existing);
                    await _unitOfWork.SaveAsync();
                }
            }

            try
            {
                await _audit.LogAsync("StudentJoinOnlineLesson", "OnlineLesson", lesson.Id.ToString(),
                    "Tələbə online dərsə qoşuldu");
            }
            catch { }

            return new JoinOnlineLessonResultDto
            {
                CanJoin = true,
                LessonId = lesson.Id,
                ClassroomUrl = $"/Student/OnlineLessons/Classroom/{lesson.Id}",
                MeetingUrl = lesson.MeetingUrl,
                Status = lesson.Status,
                Message = "ok"
            };
        }

        // ── Sorğular ──

        public async Task<IEnumerable<OnlineLessonDto>> GetTeacherGroupOnlineLessonsAsync(int groupId, int teacherId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId);
            if (group != null && group.TeacherId != teacherId)
                throw new UnauthorizedException("Bu qrupun online dərslərinə baxmaq icazəniz yoxdur");

            var lessons = await _unitOfWork.OnlineLessons.GetByGroupAsync(groupId);
            return lessons.Select(MapToDto);
        }

        public async Task<IEnumerable<OnlineLessonDto>> GetStudentOnlineLessonsAsync(int studentId)
        {
            var lessons = await _unitOfWork.OnlineLessons.GetByStudentAsync(studentId);
            return lessons.Select(MapToDto);
        }

        public async Task<IEnumerable<OnlineLessonParticipantDto>> GetOnlineLessonParticipantsAsync(int onlineLessonId)
        {
            var participants = await _unitOfWork.OnlineLessons.GetParticipantsAsync(onlineLessonId);
            return participants.Select(p => new OnlineLessonParticipantDto
            {
                Id = p.Id,
                StudentId = p.StudentId,
                StudentName = p.Student?.AppUser?.FullName ?? "",
                StudentNumber = p.Student?.StudentNumber ?? "",
                JoinedAt = p.JoinedAt,
                LeftAt = p.LeftAt,
                IsAttendanceMarked = p.IsAttendanceMarked
            });
        }

        public async Task<IEnumerable<OnlineLessonDto>> GetAllOnlineLessonsAsync()
        {
            var lessons = await _unitOfWork.OnlineLessons.GetAllWithDetailsAsync();
            return lessons.Select(MapToDto);
        }

        public async Task<OnlineLessonDto?> GetOnlineLessonAsync(int onlineLessonId, string requestingUserId)
        {
            if (!await CanAccessClassroomAsync(onlineLessonId, requestingUserId))
                return null;
            var lesson = await _unitOfWork.OnlineLessons.GetWithDetailsAsync(onlineLessonId);
            return lesson == null ? null : MapToDto(lesson);
        }

        // ── Davamiyyət inteqrasiyası ──

        public async Task<MarkPresentResultDto> MarkJoinedStudentsAsPresentAsync(int onlineLessonId, int teacherId)
        {
            var lesson = await _unitOfWork.OnlineLessons.GetWithDetailsAsync(onlineLessonId)
                ?? throw new NotFoundException("Online dərs tapılmadı");

            EnsureOwningTeacher(lesson, teacherId);

            var participants = lesson.Participants?.Where(p => !p.IsDeleted).ToList()
                ?? new List<OnlineLessonParticipant>();
            if (participants.Count == 0)
                throw new BadRequestException("Bu dərsə qoşulan tələbə yoxdur");

            // Hər iştirakçı üçün qrupdakı aktiv enrollment-i tapıb Present davamiyyət yazısı qur
            var enrollments = (await _unitOfWork.Enrollments.GetByGroupAsync(lesson.GroupId))
                .Where(e => !e.IsDeleted && e.Status == EnrollmentStatus.Active)
                .ToList();

            var dtos = new List<DTOs.Attendance.CreateAttendanceDto>();
            foreach (var p in participants)
            {
                var enrollment = enrollments.FirstOrDefault(e => e.StudentId == p.StudentId);
                if (enrollment == null) continue;

                dtos.Add(new DTOs.Attendance.CreateAttendanceDto
                {
                    EnrollmentId = enrollment.Id,
                    ClassSessionId = lesson.ClassSessionId,
                    SessionDate = lesson.ClassSession.Date.ToDateTime(TimeOnly.MinValue),
                    Status = AttendanceStatus.Present
                });
                p.IsAttendanceMarked = true;
                _unitOfWork.OnlineLessons.UpdateParticipant(p);
            }

            if (dtos.Count == 0)
                throw new BadRequestException("Qoşulan tələbələr üçün aktiv qeydiyyat tapılmadı");

            await _attendance.RecordBulkAttendanceAsync(dtos);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("MarkPresentOnlineLesson", "OnlineLesson", lesson.Id.ToString(),
                $"Qoşulan {dtos.Count} tələbə Present işarələndi");

            return new MarkPresentResultDto
            {
                MarkedCount = dtos.Count,
                Message = $"{dtos.Count} tələbə Present işarələndi"
            };
        }

        // ── Mapper ──

        private static OnlineLessonDto MapToDto(OnlineLesson o) => new()
        {
            Id = o.Id,
            GroupId = o.GroupId,
            ClassSessionId = o.ClassSessionId,
            TeacherId = o.TeacherId,
            GroupName = o.Group?.Name ?? "",
            CourseName = o.Group?.Course?.Name ?? "",
            TeacherName = o.Teacher?.AppUser?.FullName ?? "",
            SessionNumber = o.ClassSession?.SessionNumber ?? 0,
            SessionDate = o.ClassSession != null
                ? o.ClassSession.Date.ToDateTime(TimeOnly.MinValue)
                : default,
            Title = o.Title,
            Description = o.Description,
            MeetingUrl = o.MeetingUrl,
            Platform = o.Platform,
            Status = o.Status,
            StartedAt = o.StartedAt,
            EndedAt = o.EndedAt,
            ParticipantCount = o.Participants?.Count(p => !p.IsDeleted) ?? 0
        };

        // ── Köməkçilər ──

        /// <summary>Müəllimin bu dərsin sahibi olduğunu təsdiqləyir.</summary>
        private static void EnsureOwningTeacher(OnlineLesson lesson, int teacherId)
        {
            if (lesson.TeacherId != teacherId)
                throw new UnauthorizedException("Bu online dərs üzərində əməliyyat icazəniz yoxdur");
        }

        /// <summary>Başlıq boş olarsa qrup adı və sessiya nömrəsindən default qurur (≤200).</summary>
        private static string BuildDefaultTitle(string groupName, int sessionNumber)
        {
            var title = $"{groupName} — Dərs #{sessionNumber}";
            return title.Length > 200 ? title.Substring(0, 200) : title;
        }

        /// <summary>Qrupun aktiv tələbələrinin AppUserId-lərini gətirir (bildiriş üçün).</summary>
        private async Task<List<string>> GetEnrolledStudentUserIdsAsync(int groupId)
        {
            var enrollments = await _unitOfWork.Enrollments.GetByGroupAsync(groupId);
            var userIds = new List<string>();
            foreach (var e in enrollments.Where(e => !e.IsDeleted && e.Status == EnrollmentStatus.Active))
            {
                if (e.Student?.AppUserId != null)
                    userIds.Add(e.Student.AppUserId);
            }
            return userIds;
        }

        // ── Daxili Live Classroom ──

        public async Task<bool> CanAccessClassroomAsync(int onlineLessonId, string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            var lesson = await _unitOfWork.OnlineLessons.GetByIdAsync(onlineLessonId);
            if (lesson == null) return false;

            // Sahib müəllim?
            var teacher = await _unitOfWork.Teachers.GetTeacherByUserIdAsync(userId);
            if (teacher != null && lesson.TeacherId == teacher.Id) return true;

            // Aktiv enrollment olan tələbə?
            var student = await _unitOfWork.Students.GetStudentByUserIdAsync(userId);
            if (student != null)
            {
                var isEnrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(student.Id, lesson.GroupId);
                if (isEnrolled) return true;
            }

            return false;
        }

        public async Task<bool> IsClassroomTeacherAsync(int onlineLessonId, string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            var lesson = await _unitOfWork.OnlineLessons.GetByIdAsync(onlineLessonId);
            if (lesson == null) return false;

            var teacher = await _unitOfWork.Teachers.GetTeacherByUserIdAsync(userId);
            return teacher != null && lesson.TeacherId == teacher.Id;
        }

        public async Task<IEnumerable<OnlineLessonChatMessageDto>> GetClassroomChatHistoryAsync(int onlineLessonId, string requestingUserId)
        {
            if (!await CanAccessClassroomAsync(onlineLessonId, requestingUserId))
                throw new UnauthorizedException("Bu classroom-a giriş icazəniz yoxdur");

            var messages = await _unitOfWork.OnlineLessons.GetChatMessagesAsync(onlineLessonId);
            return messages.Select(m => new OnlineLessonChatMessageDto
            {
                Id = m.Id,
                OnlineLessonId = m.OnlineLessonId,
                SenderUserId = m.SenderUserId,
                SenderName = m.Sender?.FullName ?? "",
                MessageText = m.MessageText,
                SentAt = m.SentAt
            });
        }

        public async Task<OnlineLessonChatMessageDto> SaveClassroomChatMessageAsync(int onlineLessonId, string senderUserId, string text)
        {
            if (!await CanAccessClassroomAsync(onlineLessonId, senderUserId))
                throw new UnauthorizedException("Bu classroom-a mesaj göndərmək icazəniz yoxdur");

            var clean = (text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(clean))
                throw new BadRequestException("Mesaj boş ola bilməz");
            if (clean.Length > 2000) clean = clean.Substring(0, 2000);

            var msg = new OnlineLessonChatMessage
            {
                OnlineLessonId = onlineLessonId,
                SenderUserId = senderUserId,
                MessageText = clean,
                SentAt = _clock.UtcNow,
                CreatedAt = _clock.UtcNow
            };
            await _unitOfWork.OnlineLessons.AddChatMessageAsync(msg);
            await _unitOfWork.SaveAsync();

            // Sender adını UserManager-siz almaq üçün yenidən load edirik
            var saved = (await _unitOfWork.OnlineLessons.GetChatMessagesAsync(onlineLessonId, take: 1000))
                .FirstOrDefault(m => m.Id == msg.Id);

            return new OnlineLessonChatMessageDto
            {
                Id = msg.Id,
                OnlineLessonId = msg.OnlineLessonId,
                SenderUserId = msg.SenderUserId,
                SenderName = saved?.Sender?.FullName ?? "",
                MessageText = msg.MessageText,
                SentAt = msg.SentAt
            };
        }
    }
}
