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

        public DashboardController(
            IStudentApi studentApi,
            IEnrollmentApi enrollmentApi,
            IAnnouncementApi announcementApi,
            INotificationApi notificationApi)
        {
            _studentApi = studentApi;
            _enrollmentApi = enrollmentApi;
            _announcementApi = announcementApi;
            _notificationApi = notificationApi;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var student = await _studentApi.GetMeAsync();
                var enrollments = await _enrollmentApi.GetMyEnrollmentsAsync();
                var announcements = await _announcementApi.GetMyAnnouncementsAsync();
                var unreadCount = await _notificationApi.GetUnreadCountAsync();

                ViewBag.Student = student;
                ViewBag.ActiveCourses = enrollments.Count(e => e.Status == SIS.Entities.Enums.EnrollmentStatus.Active);
                ViewBag.Enrollments = enrollments.Take(5);
                ViewBag.Announcements = announcements.Take(5);
                ViewBag.UnreadCount = unreadCount.Count;
            }
            catch { }

            return View();
        }
    }
}
