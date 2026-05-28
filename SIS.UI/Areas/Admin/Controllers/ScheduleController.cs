using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.ClassSession;
using SIS.BLL.Services.DTOs.WeeklySchedule;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class ScheduleController : Controller
    {
        private readonly ICourseOfferingApi _offeringApi;
        private readonly IWeeklyScheduleApi _weeklyApi;
        private readonly IClassSessionApi _sessionApi;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(
            ICourseOfferingApi offeringApi,
            IWeeklyScheduleApi weeklyApi,
            IClassSessionApi sessionApi,
            ILogger<ScheduleController> logger)
        {
            _offeringApi = offeringApi;
            _weeklyApi = weeklyApi;
            _sessionApi = sessionApi;
            _logger = logger;
        }

        // GET: /Admin/Schedule
        public async Task<IActionResult> Index()
        {
            var offerings = await _offeringApi.GetAllAsync();
            return View(offerings);
        }

        // GET: /Admin/Schedule/Weekly/5
        public async Task<IActionResult> Weekly(int id)
        {
            var offering = await _offeringApi.GetByIdAsync(id);
            if (offering == null) return NotFound();

            var patterns = await _weeklyApi.GetByCourseAsync(id);

            ViewBag.Offering = offering;
            ViewBag.Patterns = patterns.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWeekly(
            int courseOfferingId,
            int[] dayOfWeek,
            string[] startTime,
            string[] endTime,
            string?[] room)
        {
            try
            {
                if (dayOfWeek == null || dayOfWeek.Length == 0)
                {
                    TempData["Error"] = "Ən azı bir slot əlavə edin";
                    return RedirectToAction("Weekly", new { id = courseOfferingId });
                }

                var slots = new List<WeeklySlotDto>();
                for (int i = 0; i < dayOfWeek.Length; i++)
                {
                    if (!TimeOnly.TryParse(startTime[i], out var start) ||
                        !TimeOnly.TryParse(endTime[i], out var end))
                    {
                        TempData["Error"] = "Saatlar düzgün formatda deyil";
                        return RedirectToAction("Weekly", new { id = courseOfferingId });
                    }

                    slots.Add(new WeeklySlotDto
                    {
                        DayOfWeek = (DayOfWeek)dayOfWeek[i],
                        StartTime = start,
                        EndTime = end,
                        Room = string.IsNullOrWhiteSpace(room[i]) ? null : room[i]
                    });
                }

                await _weeklyApi.SetAsync(new SetWeeklyScheduleDto
                {
                    CourseOfferingId = courseOfferingId,
                    Slots = slots
                });

                TempData["Success"] = "Həftəlik cədvəl saxlanıldı";
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx,
                    "Admin SaveWeekly error: Status={Status}, Content={Content}",
                    apiEx.StatusCode, apiEx.Content);
                TempData["Error"] = ExtractMessage(apiEx) ?? apiEx.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Weekly", new { id = courseOfferingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int id)
        {
            try
            {
                await _sessionApi.GenerateAsync(id);
                TempData["Success"] = "Sessiyalar yaradıldı";
            }
            catch (Refit.ApiException apiEx)
            {
                TempData["Error"] = ExtractMessage(apiEx) ?? apiEx.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Sessions", new { id });
        }

        // GET: /Admin/Schedule/Sessions/5
        public async Task<IActionResult> Sessions(int id)
        {
            var offering = await _offeringApi.GetByIdAsync(id);
            if (offering == null) return NotFound();

            var sessions = await _sessionApi.GetByCourseAsync(id);

            ViewBag.Offering = offering;
            ViewBag.Sessions = sessions.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSession(
            int sessionId, int courseOfferingId,
            DateOnly date, string startTime, string endTime,
            string? room, string? topic, string? notes)
        {
            try
            {
                if (!TimeOnly.TryParse(startTime, out var start) ||
                    !TimeOnly.TryParse(endTime, out var end))
                {
                    TempData["Error"] = "Saatlar düzgün formatda deyil";
                    return RedirectToAction("Sessions", new { id = courseOfferingId });
                }

                await _sessionApi.UpdateAsync(new UpdateClassSessionDto
                {
                    Id = sessionId,
                    Date = date,
                    StartTime = start,
                    EndTime = end,
                    Room = string.IsNullOrWhiteSpace(room) ? null : room,
                    Topic = string.IsNullOrWhiteSpace(topic) ? null : topic,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
                });
                TempData["Success"] = "Sessiya yeniləndi";
            }
            catch (Refit.ApiException apiEx)
            {
                TempData["Error"] = ExtractMessage(apiEx) ?? apiEx.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Sessions", new { id = courseOfferingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelSession(int sessionId, int courseOfferingId, string reason)
        {
            try
            {
                await _sessionApi.CancelAsync(new CancelClassSessionDto
                {
                    Id = sessionId,
                    Reason = reason
                });
                TempData["Success"] = "Sessiya ləğv edildi";
            }
            catch (Refit.ApiException apiEx)
            {
                TempData["Error"] = ExtractMessage(apiEx) ?? apiEx.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Sessions", new { id = courseOfferingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMakeup(
            int originalSessionId, int courseOfferingId,
            DateOnly date, string startTime, string endTime,
            string? room, string? topic)
        {
            try
            {
                if (!TimeOnly.TryParse(startTime, out var start) ||
                    !TimeOnly.TryParse(endTime, out var end))
                {
                    TempData["Error"] = "Saatlar düzgün formatda deyil";
                    return RedirectToAction("Sessions", new { id = courseOfferingId });
                }

                await _sessionApi.CreateMakeupAsync(new CreateMakeupSessionDto
                {
                    OriginalSessionId = originalSessionId,
                    Date = date,
                    StartTime = start,
                    EndTime = end,
                    Room = string.IsNullOrWhiteSpace(room) ? null : room,
                    Topic = string.IsNullOrWhiteSpace(topic) ? null : topic
                });
                TempData["Success"] = "Makeup sessiyası yaradıldı";
            }
            catch (Refit.ApiException apiEx)
            {
                TempData["Error"] = ExtractMessage(apiEx) ?? apiEx.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Sessions", new { id = courseOfferingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSession(int sessionId, int courseOfferingId)
        {
            try
            {
                await _sessionApi.DeleteAsync(sessionId);
                TempData["Success"] = "Sessiya silindi";
            }
            catch (Refit.ApiException apiEx)
            {
                TempData["Error"] = ExtractMessage(apiEx) ?? apiEx.Message;
            }
            catch
            {
                TempData["Error"] = "Silmə zamanı xəta baş verdi";
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
