using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Policy = "StudentOnly")]
    public class DashboardController : Controller
    {
        private readonly IStudentApi _studentApi;
        private readonly IEnrollmentApi _enrollmentApi;
        private readonly IAnnouncementApi _announcementApi;
        private readonly INotificationApi _notificationApi;
        private readonly ITranscriptApi _transcriptApi;

        public DashboardController(IStudentApi studentApi, IEnrollmentApi enrollmentApi,
            IAnnouncementApi announcementApi, INotificationApi notificationApi,
            ITranscriptApi transcriptApi)
        {
            _studentApi = studentApi;
            _enrollmentApi = enrollmentApi;
            _announcementApi = announcementApi;
            _notificationApi = notificationApi;
            _transcriptApi = transcriptApi;
        }

        /// <summary>UI composition üçün enrollment + onun transcript snapshot-ı.
        /// Yeni hesablama YOXDUR — yalnız mövcud transcript dəyərləri saxlanılır ki,
        /// dashboard card-larında per-course attendance/grade/progress göstərə bilək.</summary>
        public class EnrollmentCardVm
        {
            public SIS.BLL.Services.DTOs.Enrollment.EnrollmentListDto Enrollment { get; set; } = null!;
            public SIS.BLL.Services.DTOs.Transcript.TranscriptDto? Transcript { get; set; }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var student = await _studentApi.GetMeAsync();
                var enrollments = await _enrollmentApi.GetByStudentAsync(student.Id);
                var announcements = await _announcementApi.GetMyAnnouncementsAsync();
                var unreadCount = await _notificationApi.GetUnreadCountAsync();

                ViewBag.Student = student;
                ViewBag.ActiveCourses = enrollments.Count(e => e.Status == SIS.Entities.Enums.EnrollmentStatus.Active);
                ViewBag.Enrollments = enrollments.Take(5);
                ViewBag.Announcements = announcements.Take(5);
                ViewBag.UnreadCount = unreadCount.Count;

                // F6 — real akademik göstəricilər (transcript servisindən aqreqasiya).
                // Hesablama yoxdursa (məs. dərs/qiymət hələ yoxdur) sahə null qalır → UI "—" göstərir.
                double attSum = 0; int attN = 0;
                decimal gradeSum = 0; int gradeN = 0;
                int pendingAssignments = 0;
                int eligibleCount = 0; int transcriptCount = 0;

                // UI composition: per-enrollment transcript snapshot (eyni hesablama, sadəcə saxlanılır).
                var cards = new List<EnrollmentCardVm>();

                foreach (var e in enrollments)
                {
                    SIS.BLL.Services.DTOs.Transcript.TranscriptDto? t = null;
                    try
                    {
                        t = await _transcriptApi.GetAsync(e.Id);
                        var hasAtt = (t.PresentCount + t.LateCount + t.AbsentCount) > 0;
                        if (hasAtt) { attSum += t.AttendancePercentage; attN++; }
                        gradeSum += t.OfficialGrade; gradeN++;
                        if (t.CertificateEligible) eligibleCount++;
                        transcriptCount++;
                        pendingAssignments += t.HomeworkAssignments.Count(h => !h.IsSubmitted);
                    }
                    catch { /* bu enrollment üçün transcript hazır deyil */ }

                    cards.Add(new EnrollmentCardVm { Enrollment = e, Transcript = t });
                }

                ViewBag.EnrollmentCards = cards.Take(5).ToList();
                ViewBag.AttendancePct = attN > 0 ? (double?)(attSum / attN) : null;
                ViewBag.OfficialGrade = gradeN > 0 ? (decimal?)(gradeSum / gradeN) : null;
                ViewBag.PendingAssignments = pendingAssignments;
                ViewBag.CertificateEligibleCount = eligibleCount;
                ViewBag.CertificateTotal = transcriptCount;
            }
            catch { }

            return View();
        }
    }
}
