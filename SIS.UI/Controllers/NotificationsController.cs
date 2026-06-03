using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.Entities.Enums;
using SIS.UI.ApiClients;

namespace SIS.UI.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationApi _api;
        public NotificationsController(INotificationApi api) { _api = api; }

        // Tam səhifə — filter + pagination
        public async Task<IActionResult> Index(bool? isRead, NotificationType? type, int page = 1, int pageSize = 20)
        {
            var result = await _api.GetPagedAsync(isRead, type, page, pageSize);
            ViewBag.IsRead = isRead;
            ViewBag.Type = type;
            ViewBag.PageSize = pageSize;
            return View(result);
        }

        // Bell dropdown üçün JSON
        [HttpGet]
        public async Task<IActionResult> Recent(int count = 10)
        {
            try { return Json(await _api.GetRecentAsync(count)); }
            catch { return Json(Array.Empty<object>()); }
        }

        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            try { var c = await _api.GetUnreadCountAsync(); return Json(new { count = c.Count }); }
            catch { return Json(new { count = 0 }); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            try { await _api.MarkAsReadAsync(id); } catch { }
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            try { await _api.MarkAllAsReadAsync(); } catch { }
            return Ok();
        }

        // Bildirişə kliklənəndə: oxundu işarələ + uyğun səhifəyə apar
        [HttpGet]
        public async Task<IActionResult> Open(int id, string? url)
        {
            try { await _api.MarkAsReadAsync(id); } catch { }

            if (!string.IsNullOrWhiteSpace(url) && Url.IsLocalUrl(url))
                return Redirect(url);
            return RedirectToAction(nameof(Index));
        }
    }
}
