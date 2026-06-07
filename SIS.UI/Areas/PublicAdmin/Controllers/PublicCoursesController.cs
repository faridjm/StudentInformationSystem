using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.PublicAdmin;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.PublicAdmin.Controllers
{
    [Area("PublicAdmin")]
    [Authorize(Policy = "PublicAdminOnly")]
    public class PublicCoursesController : Controller
    {
        private readonly IPublicAdminApi _api;
        public PublicCoursesController(IPublicAdminApi api) { _api = api; }

        public async Task<IActionResult> Index()
        {
            try { return View(await _api.ListCourseProfilesAsync()); }
            catch { return View(new List<PublicCourseProfileRowDto>()); }
        }

        public async Task<IActionResult> Edit(int courseId)
        {
            PublicCourseProfileEditDto? dto;
            try { dto = await _api.GetCourseProfileAsync(courseId); } catch { dto = null; }
            if (dto == null) return RedirectToAction(nameof(Index));
            ViewBag.CourseId = courseId;
            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PublicCourseProfileEditDto dto)
        {
            try { await _api.UpsertCourseProfileAsync(dto); TempData["Success"] = "Public kurs profili yadda saxlanıldı."; }
            catch (Exception ex) { TempData["Error"] = "Profil saxlanılmadı: " + ex.Message; }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibility(int courseId)
        {
            try { await _api.ToggleCourseVisibilityAsync(courseId); TempData["Success"] = "Görünürlük dəyişdirildi."; }
            catch (Refit.ApiException) { TempData["Error"] = "Əvvəl public profili yaradın (Düzəliş et)."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Index));
        }
    }
}
