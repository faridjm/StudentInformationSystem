using SIS.BLL.Services.DTOs.Dashboard;
using SIS.BLL.Services.DTOs.Notification;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Time;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;

namespace SIS.BLL.Services.Implementations
{
    public class TeacherDashboardService : ITeacherDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeacherService _teacherService;
        private readonly IClock _clock;

        public TeacherDashboardService(IUnitOfWork unitOfWork, ITeacherService teacherService, IClock clock)
        {
            _unitOfWork = unitOfWork;
            _teacherService = teacherService;
            _clock = clock;
        }

        public async Task<TeacherDashboardDto> GetDashboardAsync(string userId)
        {
            var dto = new TeacherDashboardDto();
            if (string.IsNullOrEmpty(userId)) return dto;

            var teacher = await _teacherService.GetTeacherByUserIdAsync(userId);
            if (teacher == null) return dto;

            dto.TeacherName = teacher.FullName;
            var today = _clock.Today;

            var groups = (await _unitOfWork.Groups.GetByTeacherAsync(teacher.Id)).ToList();
            dto.ActiveGroupsCount = groups.Count(g => g.IsActive);

            foreach (var group in groups)
            {
                var courseName = group.Course?.Name ?? "";

                // ── Bugünkü dərslər ──
                var sessions = await _unitOfWork.ClassSessions.GetByDateRangeAsync(group.Id, today, today);
                foreach (var s in sessions.Where(x => x.Status != ClassSessionStatus.Cancelled))
                {
                    var taken = (await _unitOfWork.AttendanceRecords.GetAttendanceBySessionAsync(s.Id)).Any();
                    dto.TodayClasses.Add(new TodayClassDto
                    {
                        SessionId = s.Id,
                        GroupId = group.Id,
                        GroupName = group.Name,
                        CourseName = courseName,
                        Topic = s.Topic,
                        Date = s.Date.ToDateTime(s.StartTime),
                        StartTime = s.StartTime.ToString("HH\\:mm"),
                        EndTime = s.EndTime.ToString("HH\\:mm"),
                        Room = s.Room,
                        AttendanceTaken = taken
                    });
                }

                // ── Yoxlanılmamış tapşırıq cavabları ──
                var assignments = await _unitOfWork.Assignments.GetByGroupAsync(group.Id);
                foreach (var a in assignments)
                {
                    var pending = a.Submissions?.Count(sub => sub.SubmittedAt != null && !sub.IsGraded) ?? 0;
                    if (pending > 0)
                    {
                        dto.PendingAssignmentReviews += pending;
                        dto.PendingAssignments.Add(new PendingReviewDto
                        {
                            AssignmentId = a.Id,
                            AssignmentTitle = a.Title,
                            GroupName = group.Name,
                            GroupId = group.Id,
                            PendingCount = pending
                        });
                    }
                }

                // ── Aktiv quizlər + təhvil verilmiş nəticələr ──
                var quizzes = await _unitOfWork.Quizzes.GetByGroupAsync(group.Id);
                foreach (var q in quizzes)
                {
                    dto.SubmittedQuizResults += q.Attempts?.Count(at => at.IsSubmitted) ?? 0;
                    if (q.IsActive)
                    {
                        dto.ActiveQuizzesCount++;
                        dto.ActiveQuizzes.Add(new DashboardQuizDto
                        {
                            QuizId = q.Id,
                            Title = q.Title,
                            GroupName = group.Name,
                            GroupId = group.Id,
                            AttemptCount = q.Attempts?.Count(at => at.IsSubmitted) ?? 0
                        });
                    }
                }

                // ── Vaxtı çatmış, qiymətləndirilməmiş rəsmi qiymətlər ──
                var items = await _unitOfWork.AssessmentItems.GetByGroupAsync(group.Id);
                foreach (var item in items.Where(i => DateOnly.FromDateTime(i.Date) <= today))
                {
                    var subs = await _unitOfWork.AssessmentSubmissions.GetByAssessmentItemAsync(item.Id);
                    dto.PendingGrades += subs.Count(sub => !sub.IsGraded);
                }
            }

            dto.TodayClassesCount = dto.TodayClasses.Count;
            dto.AttendanceNotTakenCount = dto.TodayClasses.Count(c => !c.AttendanceTaken);

            // ── Oxunmamış mesajlar ──
            dto.UnreadMessages = await _unitOfWork.Chat.GetUnreadCountAsync(userId);

            // ── Son bildirişlər ──
            var notifs = await _unitOfWork.Notifications.GetRecentAsync(userId, 5);
            dto.RecentNotifications = notifs.Select(n => new NotificationDto
            {
                Id = n.Id, Title = n.Title, Message = n.Message,
                Type = n.Type, IsRead = n.IsRead, RelatedUrl = n.RelatedUrl,
                CreatedAt = n.CreatedAt, ReadAt = n.ReadAt
            }).ToList();

            return dto;
        }
    }
}
