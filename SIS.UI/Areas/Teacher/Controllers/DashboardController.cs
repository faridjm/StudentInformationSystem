using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.UI.ApiClients;


namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class DashboardController : Controller
    {
        private readonly ITeacherApi _teacherApi;
        private readonly ICourseOfferingApi _offeringApi;
        private readonly IAnnouncementApi _announcementApi;

        public DashboardController(
            ITeacherApi teacherApi,
            ICourseOfferingApi offeringApi,
            IAnnouncementApi announcementApi)
        {
            _teacherApi = teacherApi;
            _offeringApi = offeringApi;
            _announcementApi = announcementApi;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var teacher = await _teacherApi.GetMeAsync();
                var offerings = await _offeringApi.GetByTeacherAsync(teacher.Id);
                var announcements = await _announcementApi.GetMyAnnouncementsAsync();

                ViewBag.Teacher = teacher;
                ViewBag.CourseCount = offerings.Count();
                ViewBag.Announcements = announcements.Take(5);
                ViewBag.Offerings = offerings;
            }
            catch { }

            return View();
        }
    }
}
