using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.Common.Constants;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public class ArchiveController : Controller
    {
        private readonly IArchiveApi _api;
        public ArchiveController(IArchiveApi api) { _api = api; }

        // GET /Admin/Archive
        public IActionResult Index() => RedirectToAction("Chats");

        // GET /Admin/Archive/Chats
        public async Task<IActionResult> Chats(
            string? search, string? role, string? status,
            DateTime? fromDate, DateTime? toDate, int? groupId,
            int page = 1, int pageSize = 25)
        {
            var result = await _api.GetChatsAsync(search, role, status, fromDate, toDate, groupId, page, pageSize);

            ViewBag.Search    = search;
            ViewBag.Role      = role;
            ViewBag.Status    = status;
            ViewBag.FromDate  = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate    = toDate?.ToString("yyyy-MM-dd");
            ViewBag.PageSize  = pageSize;

            return View(result);
        }

        // GET /Admin/Archive/Conversation/{id}
        public async Task<IActionResult> Conversation(int id)
        {
            try
            {
                var d = await _api.GetDetailAsync(id);
                return View(d);
            }
            catch
            {
                TempData["Error"] = "Söhbət tapılmadı";
                return RedirectToAction("Chats");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            try { await _api.ArchiveAsync(id); TempData["Success"] = "Söhbət arxivləşdirildi"; }
            catch { TempData["Error"] = "Əməliyyat baş tutmadı"; }
            return RedirectToAction("Conversation", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            try { await _api.RestoreAsync(id); TempData["Success"] = "Söhbət bərpa edildi"; }
            catch { TempData["Error"] = "Əməliyyat baş tutmadı"; }
            return RedirectToAction("Conversation", new { id });
        }
    }
}
