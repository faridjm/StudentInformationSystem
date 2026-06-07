using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.PublicAdmin;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.PublicAdmin.Controllers
{
    [Area("PublicAdmin")]
    [Authorize(Policy = "PublicAdminOnly")]
    public class FacilitiesController : Controller
    {
        private readonly IPublicAdminApi _api;
        public FacilitiesController(IPublicAdminApi api) { _api = api; }

        public async Task<IActionResult> Index()
        {
            try { return View(await _api.ListFacilitiesAsync(false)); }
            catch { return View(new List<PublicFacilityDto>()); }
        }

        public IActionResult Create() => View("Edit", new PublicFacilityDto { IsVisible = true, SortOrder = 0 });

        public async Task<IActionResult> Edit(int id)
        {
            try { var d = await _api.GetFacilityAsync(id); return View(d); }
            catch { return RedirectToAction(nameof(Index)); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(PublicFacilityDto dto)
        {
            try
            {
                if (dto.Id == 0)
                {
                    var resp = await _api.CreateFacilityAsync(dto);
                    if (!resp.IsSuccessStatusCode) TempData["Error"] = "Yaradıla bilmədi.";
                    else TempData["Success"] = "Facility yaradıldı.";
                }
                else
                {
                    await _api.UpdateFacilityAsync(dto);
                    TempData["Success"] = "Facility yeniləndi.";
                }
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try { await _api.DeleteFacilityAsync(id); TempData["Success"] = "Silindi."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Index));
        }
    }
}
