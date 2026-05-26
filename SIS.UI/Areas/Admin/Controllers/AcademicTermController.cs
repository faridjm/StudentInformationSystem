using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.AcademicTerm;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AcademicTermController : Controller
    {
        private readonly IAcademicTermApi _termApi;
        private readonly IAcademicYearApi _yearApi;
        private readonly ILogger<AcademicTermController> _logger;

        public AcademicTermController(
            IAcademicTermApi termApi,
            IAcademicYearApi yearApi,
            ILogger<AcademicTermController> logger)
        {
            _termApi = termApi;
            _yearApi = yearApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var terms = await _termApi.GetAllAsync();
            return View(terms);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Years = await _yearApi.GetAllAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAcademicTermDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Years = await _yearApi.GetAllAsync();
                return View(dto);
            }
            try
            {
                await _termApi.CreateAsync(dto);
                TempData["Success"] = "Semestr uğurla yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx,
                    "AcademicTerm Create error: Status={Status}, Content={Content}",
                    apiEx.StatusCode, apiEx.Content);
                ModelState.AddModelError("", ExtractMessage(apiEx) ?? apiEx.Message);
                ViewBag.Years = await _yearApi.GetAllAsync();
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Years = await _yearApi.GetAllAsync();
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var term = await _termApi.GetByIdAsync(id);
            if (term == null) return NotFound();

            var dto = new UpdateAcademicTermDto
            {
                Id = term.Id,
                Name = term.Name,
                AcademicYearId = term.AcademicYearId ?? 0,
                RegistrationStart = term.RegistrationStart ?? term.StartDate.AddDays(-14),
                RegistrationEnd = term.RegistrationEnd ?? term.StartDate.AddDays(-1),
                StartDate = term.StartDate,
                DropDeadline = term.DropDeadline ?? term.StartDate.AddDays(30),
                EndDate = term.EndDate,
                ExamPeriodStart = term.ExamPeriodStart ?? term.EndDate.AddDays(-14),
                ExamPeriodEnd = term.ExamPeriodEnd ?? term.EndDate,
                GradeSubmissionDeadline = term.GradeSubmissionDeadline ?? term.EndDate.AddDays(7)
            };

            ViewBag.Years = await _yearApi.GetAllAsync();
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateAcademicTermDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Years = await _yearApi.GetAllAsync();
                return View(dto);
            }
            try
            {
                await _termApi.UpdateAsync(dto);
                TempData["Success"] = "Semestr uğurla yeniləndi";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx,
                    "AcademicTerm Edit error: Status={Status}, Content={Content}",
                    apiEx.StatusCode, apiEx.Content);
                ModelState.AddModelError("", ExtractMessage(apiEx) ?? apiEx.Message);
                ViewBag.Years = await _yearApi.GetAllAsync();
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Years = await _yearApi.GetAllAsync();
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetActive(int id)
        {
            try
            {
                await _termApi.SetActiveAsync(id);
                TempData["Success"] = "Semestr aktiv edildi";
            }
            catch { TempData["Error"] = "Xəta baş verdi"; }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _termApi.DeleteAsync(id);
                TempData["Success"] = "Semestr uğurla silindi";
            }
            catch { TempData["Error"] = "Silmə zamanı xəta baş verdi"; }
            return RedirectToAction("Index");
        }

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
