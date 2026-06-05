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

        public AnnouncementsController(IAnnouncementApi announcementApi)
        {
            _announcementApi = announcementApi;
        }

        public async Task<IActionResult> Index()
        {
            var announcements = await _announcementApi.GetMyTeacherAnnouncementsAsync();
            return View(announcements);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try { await _announcementApi.MarkAsReadAsync(id); }
            catch { /* read xətası user axınını pozmasın */ }
            return Ok();
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAnnouncementDto dto)
        {
            await _announcementApi.CreateAsync(dto);
            TempData["Success"] = "Elan yayımlandı";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _announcementApi.DeleteAsync(id);
            TempData["Success"] = "Elan silindi";
            return RedirectToAction("Index");
        }
    }
}
