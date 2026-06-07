using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.PublicAdmin;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.PublicAdmin.Controllers
{
    [Area("PublicAdmin")]
    [Authorize(Policy = "PublicAdminOnly")]
    public class PublicTeachersController : Controller
    {
        private readonly IPublicAdminApi _api;
        public PublicTeachersController(IPublicAdminApi api) { _api = api; }

        public async Task<IActionResult> Index()
        {
            try { return View(await _api.ListTeacherProfilesAsync()); }
            catch { return View(new List<PublicTeacherProfileRowDto>()); }
        }

        public async Task<IActionResult> Edit(int teacherId)
        {
            PublicTeacherProfileEditDto? dto;
            try { dto = await _api.GetTeacherProfileAsync(teacherId); } catch { dto = null; }
            if (dto == null) return RedirectToAction(nameof(Index));
            ViewBag.TeacherId = teacherId;
            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PublicTeacherProfileEditDto dto)
        {
            try { await _api.UpsertTeacherProfileAsync(dto); TempData["Success"] = "Public müəllim profili yadda saxlanıldı."; }
            catch (Exception ex) { TempData["Error"] = "Profil saxlanılmadı: " + ex.Message; }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibility(int teacherId)
        {
            try { await _api.ToggleTeacherVisibilityAsync(teacherId); TempData["Success"] = "Görünürlük dəyişdirildi."; }
            catch (Refit.ApiException) { TempData["Error"] = "Əvvəl public profili yaradın (Düzəliş et)."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Index));
        }
    }
}
