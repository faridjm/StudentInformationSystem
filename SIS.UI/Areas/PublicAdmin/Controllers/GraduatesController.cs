using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.PublicAdmin;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.PublicAdmin.Controllers
{
    [Area("PublicAdmin")]
    [Authorize(Policy = "PublicAdminOnly")]
    public class GraduatesController : Controller
    {
        private readonly IPublicAdminApi _api;
        public GraduatesController(IPublicAdminApi api) { _api = api; }

        public async Task<IActionResult> Index()
        {
            try { return View(await _api.ListGraduatesAsync(false)); }
            catch { return View(new List<PublicGraduateStoryDto>()); }
        }

        public IActionResult Create() => View("Edit", new PublicGraduateStoryDto { IsVisible = true });

        public async Task<IActionResult> Edit(int id)
        {
            try { var d = await _api.GetGraduateAsync(id); return View(d); }
            catch { return RedirectToAction(nameof(Index)); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(PublicGraduateStoryDto dto)
        {
            try
            {
                if (dto.Id == 0)
                {
                    var resp = await _api.CreateGraduateAsync(dto);
                    if (!resp.IsSuccessStatusCode) TempData["Error"] = "Yaradıla bilmədi.";
                    else TempData["Success"] = "Məzun hekayəsi yaradıldı.";
                }
                else
                {
                    await _api.UpdateGraduateAsync(dto);
                    TempData["Success"] = "Yeniləndi.";
                }
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try { await _api.DeleteGraduateAsync(id); TempData["Success"] = "Silindi."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Index));
        }
    }
}
