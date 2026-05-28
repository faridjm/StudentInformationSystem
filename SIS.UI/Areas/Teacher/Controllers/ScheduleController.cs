using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.ClassSession;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class ScheduleController : Controller
    {
        private readonly ICourseOfferingApi _offeringApi;
        private readonly IClassSessionApi _sessionApi;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(
            ICourseOfferingApi offeringApi,
            IClassSessionApi sessionApi,
            ILogger<ScheduleController> logger)
        {
            _offeringApi = offeringApi;
            _sessionApi = sessionApi;
            _logger = logger;
        }

        // GET: /Teacher/Schedule/Sessions/5
        public async Task<IActionResult> Sessions(int id)
        {
            var offering = await _offeringApi.GetByIdAsync(id);
            if (offering == null) return NotFound();

            var sessions = await _sessionApi.GetByCourseAsync(id);

            ViewBag.Offering = offering;
            ViewBag.Sessions = sessions.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToList();
            return View();
        }

        // POST: /Teacher/Schedule/UpdateTopic
        // Müəllim yalnız mövzu və qeydləri redaktə edə bilər.
        // Tarix, saat, otaq dəyişməsi admin-də olur.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTopic(
            int sessionId, int courseOfferingId,
            string? topic, string? notes)
        {
            try
            {
                // Cari sessiyanı götür ki, tarix/saat/otaq dəyişməsin
                var existing = await _sessionApi.GetByIdAsync(sessionId);
                if (existing == null)
                {
                    TempData["Error"] = "Sessiya tapılmadı";
                    return RedirectToAction("Sessions", new { id = courseOfferingId });
                }

                await _sessionApi.UpdateAsync(new UpdateClassSessionDto
                {
                    Id = sessionId,
                    Date = existing.Date,
                    StartTime = existing.StartTime,
                    EndTime = existing.EndTime,
                    Room = existing.Room,
                    Topic = string.IsNullOrWhiteSpace(topic) ? null : topic,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
                });
                TempData["Success"] = "Mövzu və qeydlər saxlanıldı";
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx,
                    "Teacher UpdateTopic error: Status={Status}, Content={Content}",
                    apiEx.StatusCode, apiEx.Content);
                TempData["Error"] = ExtractMessage(apiEx) ?? apiEx.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Sessions", new { id = courseOfferingId });
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
