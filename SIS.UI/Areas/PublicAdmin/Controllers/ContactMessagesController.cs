using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.PublicAdmin;
using SIS.Entities.Enums;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.PublicAdmin.Controllers
{
    [Area("PublicAdmin")]
    [Authorize(Policy = "PublicAdminOnly")]
    public class ContactMessagesController : Controller
    {
        private readonly IPublicAdminApi _api;
        public ContactMessagesController(IPublicAdminApi api) { _api = api; }

        public async Task<IActionResult> Index(ContactMessageStatus? status = null)
        {
            try { return View(await _api.ListContactAsync(status)); }
            catch { return View(new List<PublicContactMessageListDto>()); }
        }

        public async Task<IActionResult> Details(int id)
        {
            PublicContactMessageDetailDto? d;
            try { d = await _api.GetContactAsync(id); } catch { d = null; }
            if (d == null) return RedirectToAction(nameof(Index));
            // Auto-mark as Read when opened (if New)
            if (d.Status == ContactMessageStatus.New)
            {
                try { await _api.SetContactStatusAsync(id, new UpdateContactStatusDto { Status = ContactMessageStatus.Read });
                      d.Status = ContactMessageStatus.Read; }
                catch { }
            }
            return View(d);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ContactMessageStatus status)
        {
            try { await _api.SetContactStatusAsync(id, new UpdateContactStatusDto { Status = status });
                  TempData["Success"] = "Status yeniləndi."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNote(int id, string? adminNote)
        {
            try { await _api.SetContactNoteAsync(id, new UpdateContactNoteDto { AdminNote = adminNote });
                  TempData["Success"] = "Qeyd yadda saxlanıldı."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
