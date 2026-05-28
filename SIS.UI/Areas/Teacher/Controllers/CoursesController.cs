using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Attendance;
using SIS.BLL.Services.DTOs.Grade;
using SIS.Common.Time;
using SIS.UI.ApiClients;


namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class CoursesController : Controller
    {
        private readonly ITeacherApi _teacherApi;
        private readonly ICourseOfferingApi _offeringApi;
        private readonly IEnrollmentApi _enrollmentApi;
        private readonly IGradeApi _gradeApi;
        private readonly IAttendanceApi _attendanceApi;
        private readonly IClassSessionApi _sessionApi;
        private readonly IClock _clock;

        public CoursesController(
            ITeacherApi teacherApi,
            ICourseOfferingApi offeringApi,
            IEnrollmentApi enrollmentApi,
            IGradeApi gradeApi,
            IAttendanceApi attendanceApi,
            IClassSessionApi sessionApi,
            IClock clock)
        {
            _teacherApi = teacherApi;
            _offeringApi = offeringApi;
            _enrollmentApi = enrollmentApi;
            _gradeApi = gradeApi;
            _attendanceApi = attendanceApi;
            _sessionApi = sessionApi;
            _clock = clock;
        }

        public async Task<IActionResult> Index()
        {
            var teacher = await _teacherApi.GetMeAsync();
            var offerings = await _offeringApi.GetByTeacherAsync(teacher.Id);
            return View(offerings);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var offering = await _offeringApi.GetByIdAsync(id);
            if (offering == null) return NotFound();

            var enrollments = await _enrollmentApi.GetByCourseAsync(id);

            ViewBag.Offering = offering;
            ViewBag.Enrollments = enrollments;
            return View();
        }

        public async Task<IActionResult> Grades(int id)
        {
            var offering = await _offeringApi.GetByIdAsync(id);
            if (offering == null) return NotFound();

            var enrollments = await _enrollmentApi.GetByCourseAsync(id);
            ViewBag.Offering = offering;
            ViewBag.Enrollments = enrollments;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddGrade(CreateGradeDto dto, int courseOfferingId)
        {
            try
            {
                await _gradeApi.AddAsync(dto);
                TempData["Success"] = "Qiymət uğurla əlavə edildi";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Grades", new { id = courseOfferingId });
        }

        public async Task<IActionResult> Attendance(int id, int? sessionId = null)
        {
            var offering = await _offeringApi.GetByIdAsync(id);
            if (offering == null) return NotFound();

            var sessions = (await _sessionApi.GetByCourseAsync(id)).ToList();
            var enrollments = (await _enrollmentApi.GetByCourseAsync(id)).ToList();

            // Default: ən son keçmiş və ya bu günkü sessiya
            var today = _clock.Today;
            SIS.BLL.Services.DTOs.ClassSession.ClassSessionDto? selectedSession = null;
            if (sessionId.HasValue)
            {
                selectedSession = sessions.FirstOrDefault(s => s.Id == sessionId.Value);
            }
            else
            {
                selectedSession = sessions
                    .Where(s => s.Date <= today && s.Status != SIS.Entities.Enums.ClassSessionStatus.Cancelled)
                    .OrderByDescending(s => s.Date)
                    .ThenByDescending(s => s.StartTime)
                    .FirstOrDefault();
            }

            // Seçilmiş sessiya üçün mövcud davamiyyət qeydləri
            var existingStatuses = new Dictionary<int, SIS.Entities.Enums.AttendanceStatus>();
            if (selectedSession != null)
            {
                foreach (var e in enrollments)
                {
                    var records = await _attendanceApi.GetByEnrollmentAsync(e.Id);
                    var rec = records.FirstOrDefault(r => r.ClassSessionId == selectedSession.Id);
                    if (rec != null) existingStatuses[e.Id] = rec.Status;
                }
            }

            ViewBag.Offering = offering;
            ViewBag.Enrollments = enrollments;
            ViewBag.Sessions = sessions.OrderByDescending(s => s.Date).ThenByDescending(s => s.StartTime).ToList();
            ViewBag.SelectedSession = selectedSession;
            ViewBag.ExistingStatuses = existingStatuses;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordAttendance(
            int courseOfferingId,
            int sessionId,
            List<int> enrollmentIds,
            List<int> statuses)
        {
            try
            {
                if (enrollmentIds == null || enrollmentIds.Count == 0)
                {
                    TempData["Error"] = "Tələbə siyahısı boşdur";
                    return RedirectToAction("Attendance", new { id = courseOfferingId, sessionId });
                }

                var session = await _sessionApi.GetByIdAsync(sessionId);
                if (session == null)
                {
                    TempData["Error"] = "Sessiya tapılmadı";
                    return RedirectToAction("Attendance", new { id = courseOfferingId });
                }

                var dtos = enrollmentIds.Select((enrollmentId, i) =>
                    new CreateAttendanceDto
                    {
                        EnrollmentId = enrollmentId,
                        SessionDate = session.Date.ToDateTime(TimeOnly.MinValue),
                        Status = (SIS.Entities.Enums.AttendanceStatus)statuses[i],
                        ClassSessionId = sessionId
                    }).ToList();

                await _attendanceApi.RecordBulkAsync(dtos);
                TempData["Success"] = "Davamiyyət uğurla qeyd edildi";
            }
            catch (Refit.ApiException apiEx)
            {
                TempData["Error"] = ExtractAttendanceMessage(apiEx) ?? apiEx.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Attendance", new { id = courseOfferingId, sessionId });
        }

        private static string? ExtractAttendanceMessage(Refit.ApiException apiEx)
        {
            if (string.IsNullOrEmpty(apiEx.Content)) return null;
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(apiEx.Content);
                var root = json.RootElement;
                if (root.TryGetProperty("message", out var msg))
                    return msg.GetString();
                if (root.TryGetProperty("title", out var title))
                    return title.GetString();
            }
            catch { }
            return null;
        }
    }
}
