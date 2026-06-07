using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.PublicAdmin;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.PublicAdmin.Controllers
{
    [Area("PublicAdmin")]
    [Authorize(Policy = "PublicAdminOnly")]
    public class FaqController : Controller
    {
        private readonly IPublicAdminApi _api;
        public FaqController(IPublicAdminApi api) { _api = api; }

        public async Task<IActionResult> Index()
        {
            try { return View(await _api.ListFaqsAsync(false)); }
            catch { return View(new List<PublicFaqDto>()); }
        }

        public IActionResult Create() => View("Edit", new PublicFaqDto { IsVisible = true });

        public async Task<IActionResult> Edit(int id)
        {
            try { var d = await _api.GetFaqAsync(id); return View(d); }
            catch { return RedirectToAction(nameof(Index)); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(PublicFaqDto dto)
        {
            try
            {
                if (dto.Id == 0)
                {
                    var resp = await _api.CreateFaqAsync(dto);
                    if (!resp.IsSuccessStatusCode) TempData["Error"] = "Yaradıla bilmədi.";
                    else TempData["Success"] = "FAQ yaradıldı.";
                }
                else
                {
                    await _api.UpdateFaqAsync(dto);
                    TempData["Success"] = "FAQ yeniləndi.";
                }
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try { await _api.DeleteFaqAsync(id); TempData["Success"] = "Silindi."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Index));
        }
    }
}
