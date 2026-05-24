using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Announcement;
using SIS.UI.ApiClients;


namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class AnnouncementsController : Controller
    {
        private readonly IAnnouncementApi _announcementApi;
        private readonly ITeacherApi _teacherApi;
        private readonly ICourseOfferingApi _offeringApi;

        public AnnouncementsController(
            IAnnouncementApi announcementApi,
            ITeacherApi teacherApi,
            ICourseOfferingApi offeringApi)
        {
            _announcementApi = announcementApi;
            _teacherApi = teacherApi;
            _offeringApi = offeringApi;
        }

        public async Task<IActionResult> Index()
        {
            var announcements = await _announcementApi.GetMyAnnouncementsAsync();
            return View(announcements);
        }

        public async Task<IActionResult> Create()
        {
            var teacher = await _teacherApi.GetMeAsync();
            var offerings = await _offeringApi.GetByTeacherAsync(teacher.Id);
            ViewBag.Offerings = offerings;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAnnouncementDto dto)
        {
            if (!ModelState.IsValid)
            {
                var teacher = await _teacherApi.GetMeAsync();
                var offerings = await _offeringApi.GetByTeacherAsync(teacher.Id);
                ViewBag.Offerings = offerings;
                return View(dto);
            }
            try
            {
                await _announcementApi.CreateAsync(dto);
                TempData["Success"] = "Elan uğurla yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var teacher = await _teacherApi.GetMeAsync();
                var offerings = await _offeringApi.GetByTeacherAsync(teacher.Id);
                ViewBag.Offerings = offerings;
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _announcementApi.DeleteAsync(id);
                TempData["Success"] = "Elan uğurla silindi";
            }
            catch { TempData["Error"] = "Xəta baş verdi"; }
            return RedirectToAction("Index");
        }
    }

}
