using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Announcement;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AnnouncementController : Controller
    {
        private readonly IAnnouncementApi _announcementApi;

        public AnnouncementController(IAnnouncementApi announcementApi)
        {
            _announcementApi = announcementApi;
        }

        public async Task<IActionResult> Index()
        {
            var announcements = await _announcementApi.GetGlobalAsync();
            return View(announcements);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateAnnouncementDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _announcementApi.CreateAsync(dto);
                TempData["Success"] = "Elan uğurla yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
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
