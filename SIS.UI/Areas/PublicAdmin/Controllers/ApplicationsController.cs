using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Common;
using SIS.BLL.Services.DTOs.Public;
using SIS.Entities.Enums;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.PublicAdmin.Controllers
{
    [Area("PublicAdmin")]
    [Authorize(Policy = "PublicAdminOnly")]
    public class ApplicationsController : Controller
    {
        private readonly ICourseApplicationApi _api;
        private readonly ICourseApi _courseApi;
        public ApplicationsController(ICourseApplicationApi api, ICourseApi courseApi)
        { _api = api; _courseApi = courseApi; }

        public async Task<IActionResult> Index(string? search, CourseApplicationStatus? status,
            int? courseId, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            PagedResult<CourseApplicationListDto> result;
            try { result = await _api.ListAsync(search, status, courseId, fromDate, toDate, page, 20); }
            catch { result = new PagedResult<CourseApplicationListDto> { Page = page, PageSize = 20 }; }

            CourseApplicationStatsDto stats;
            try { stats = await _api.StatsAsync(); } catch { stats = new CourseApplicationStatsDto(); }

            try { ViewBag.Courses = await _courseApi.GetAllAsync(); } catch { ViewBag.Courses = Array.Empty<object>(); }
            ViewBag.Search = search; ViewBag.Status = status; ViewBag.CourseId = courseId;
            ViewBag.FromDate = fromDate; ViewBag.ToDate = toDate; ViewBag.Stats = stats;
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            CourseApplicationDetailDto? dto;
            try { dto = await _api.GetAsync(id); } catch { dto = null; }
            if (dto == null) return RedirectToAction(nameof(Index));
            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, CourseApplicationStatus status)
        {
            try { await _api.UpdateStatusAsync(id, new UpdateApplicationStatusDto { Status = status });
                  TempData["Success"] = "Status yeniləndi."; }
            catch (Refit.ApiException ax) { TempData["Error"] = "Status yenilənmədi: " + ax.Content; }
            catch (Exception ex) { TempData["Error"] = "Status yenilənmədi: " + ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNote(int id, string? adminNote)
        {
            try { await _api.UpdateNoteAsync(id, new UpdateApplicationNoteDto { AdminNote = adminNote });
                  TempData["Success"] = "Qeyd yadda saxlanıldı."; }
            catch (Exception ex) { TempData["Error"] = "Qeyd saxlanılmadı: " + ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Convert(int id, string fullName, string email, string? phone,
            string password, int enrollmentYear, bool enrollToGroup = false)
        {
            try
            {
                var res = await _api.ConvertAsync(id, new ConvertApplicationToStudentDto
                {
                    FullName = fullName, Email = email, Phone = phone,
                    Password = password, EnrollmentYear = enrollmentYear, EnrollToGroup = enrollToGroup
                });
                TempData["Success"] = res.Enrolled
                    ? $"Tələbə yaradıldı və qrupa əlavə edildi (StudentNumber: {res.StudentNumber})."
                    : (res.EnrollmentMessage != null
                        ? $"Tələbə yaradıldı (StudentNumber: {res.StudentNumber}), lakin qrupa əlavə edilmədi: {res.EnrollmentMessage}"
                        : $"Tələbə yaradıldı (StudentNumber: {res.StudentNumber}).");
            }
            catch (Refit.ApiException ax) { TempData["Error"] = "Tələbə yaradıla bilmədi: " + ax.Content; }
            catch (Exception ex) { TempData["Error"] = "Tələbə yaradıla bilmədi: " + ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
