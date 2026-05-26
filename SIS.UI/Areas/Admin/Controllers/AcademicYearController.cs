using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.AcademicYear;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AcademicYearController : Controller
    {
        private readonly IAcademicYearApi _yearApi;
        private readonly ILogger<AcademicYearController> _logger;

        public AcademicYearController(
            IAcademicYearApi yearApi,
            ILogger<AcademicYearController> logger)
        {
            _yearApi = yearApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var years = await _yearApi.GetAllAsync();
            return View(years);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateAcademicYearDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _yearApi.CreateAsync(dto);
                TempData["Success"] = "Dərs sezonu uğurla yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx,
                    "AcademicYear Create error: Status={Status}, Content={Content}",
                    apiEx.StatusCode, apiEx.Content);
                ModelState.AddModelError("", ExtractMessage(apiEx) ?? apiEx.Message);
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var year = await _yearApi.GetByIdAsync(id);
            if (year == null) return NotFound();
            var dto = new UpdateAcademicYearDto
            {
                Id = year.Id,
                Name = year.Name,
                StartDate = year.StartDate,
                EndDate = year.EndDate
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateAcademicYearDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _yearApi.UpdateAsync(dto);
                TempData["Success"] = "Dərs sezonu uğurla yeniləndi";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx,
                    "AcademicYear Edit error: Status={Status}, Content={Content}",
                    apiEx.StatusCode, apiEx.Content);
                ModelState.AddModelError("", ExtractMessage(apiEx) ?? apiEx.Message);
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetActive(int id)
        {
            try
            {
                await _yearApi.SetActiveAsync(id);
                TempData["Success"] = "Dərs sezonu aktiv edildi";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Xəta baş verdi: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _yearApi.DeleteAsync(id);
                TempData["Success"] = "Dərs sezonu uğurla silindi";
            }
            catch (Refit.ApiException apiEx)
            {
                TempData["Error"] = ExtractMessage(apiEx) ?? "Silmə zamanı xəta baş verdi";
            }
            catch
            {
                TempData["Error"] = "Silmə zamanı xəta baş verdi";
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// API-dən qayıdan xəta cavabından insan oxunaqlı mesaj çıxar.
        /// 3 formatı dəstəkləyir:
        ///   1) {"message":"..."} — custom AppException
        ///   2) {"errors":{"Field":["..."]}} — ASP.NET ModelState/FluentValidation
        ///   3) {"title":"..."} — fallback ProblemDetails
        /// </summary>
        private static string? ExtractMessage(Refit.ApiException apiEx)
        {
            if (string.IsNullOrEmpty(apiEx.Content)) return null;
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(apiEx.Content);
                var root = json.RootElement;

                if (root.TryGetProperty("message", out var msg))
                {
                    var m = msg.GetString();
                    if (!string.IsNullOrWhiteSpace(m)) return m;
                }

                if (root.TryGetProperty("errors", out var errors) &&
                    errors.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    var messages = new List<string>();
                    foreach (var field in errors.EnumerateObject())
                    {
                        if (field.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            foreach (var item in field.Value.EnumerateArray())
                            {
                                var s = item.GetString();
                                if (!string.IsNullOrWhiteSpace(s)) messages.Add(s);
                            }
                        }
                    }
                    if (messages.Count > 0) return string.Join(" • ", messages);
                }

                if (root.TryGetProperty("title", out var title))
                    return title.GetString();
            }
            catch { }
            return null;
        }
    }
}
