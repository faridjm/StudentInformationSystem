using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.Common.Time;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Policy = "StudentOnly")]
    public class CoursesController : Controller
    {
        private readonly IEnrollmentApi _enrollmentApi;
        private readonly IGradeApi _gradeApi;
        private readonly IAttendanceApi _attendanceApi;
        private readonly IStudentApi _studentApi;
        private readonly IAssessmentApi _assessmentApi;
        private readonly IAssignmentApi _assignmentApi;
        private readonly IClassSessionApi _sessionApi;
        private readonly IAnnouncementApi _announcementApi;
        private readonly IClock _clock;
        private readonly ITranscriptApi _transcriptApi;
        private readonly ICourseMaterialApi _materialApi;
        private readonly IOnlineLessonApi _onlineLessonApi;

        public CoursesController(IEnrollmentApi enrollmentApi, IGradeApi gradeApi,
            IAttendanceApi attendanceApi, IStudentApi studentApi,
            IAssessmentApi assessmentApi, IAssignmentApi assignmentApi,
            IClassSessionApi sessionApi, IAnnouncementApi announcementApi,
            IClock clock, ITranscriptApi transcriptApi, ICourseMaterialApi materialApi,
            IOnlineLessonApi onlineLessonApi)
        {
            _enrollmentApi = enrollmentApi;
            _gradeApi = gradeApi;
            _attendanceApi = attendanceApi;
            _studentApi = studentApi;
            _assessmentApi = assessmentApi;
            _assignmentApi = assignmentApi;
            _sessionApi = sessionApi;
            _announcementApi = announcementApi;
            _clock = clock;
            _transcriptApi = transcriptApi;
            _materialApi = materialApi;
            _onlineLessonApi = onlineLessonApi;
        }

        // Bildiriş linkindən: groupId-ə görə tələbənin qeydiyyatını tapıb materiallar tabına yönləndir
        public async Task<IActionResult> MaterialsByGroup(int groupId)
        {
            var enrollments = await _enrollmentApi.GetMyEnrollmentsAsync();
            var match = enrollments.FirstOrDefault(e => e.GroupId == groupId);
            if (match == null) return RedirectToAction("Index");
            return RedirectToAction("Detail", new { id = match.Id, tab = "materials" });
        }

        public async Task<IActionResult> Index()
        {
            var enrollments = await _enrollmentApi.GetMyEnrollmentsAsync();
            return View(enrollments);
        }

        public async Task<IActionResult> Detail(int id, string tab = "overview")
        {
            // id = enrollmentId
            var enrollments = (await _enrollmentApi.GetMyEnrollmentsAsync()).ToList();
            var enrollment = enrollments.FirstOrDefault(e => e.Id == id);
            if (enrollment == null) return RedirectToAction("Index");

            var attendance = (await _attendanceApi.GetByEnrollmentAsync(id)).ToList();
            var summary = await _attendanceApi.GetSummaryAsync(id);

            // Rəsmi qiymətləndirmə
            var assessmentSubs = new List<SIS.BLL.Services.DTOs.Assessment.AssessmentSubmissionDto>();
            try { assessmentSubs = (await _assessmentApi.GetMySubmissionsAsync(id)).ToList(); } catch { }

            // Ev tapşırıqları
            var assignmentSubs = new List<SIS.BLL.Services.DTOs.Assignment.SubmissionDto>();
            try { assignmentSubs = (await _assignmentApi.GetMySubmissionsAsync(id)).ToList(); } catch { }

            // Dərs cədvəli
            var sessions = new List<SIS.BLL.Services.DTOs.ClassSession.ClassSessionDto>();
            try { sessions = (await _sessionApi.GetByGroupAsync(enrollment.GroupId)).OrderBy(s => s.Date).ToList(); } catch { }

            // Elanlar
            var announcements = new List<SIS.BLL.Services.DTOs.Announcement.AnnouncementDto>();
            try { announcements = (await _announcementApi.GetByGroupAsync(enrollment.GroupId)).ToList(); } catch { }

            // Tapşırıqlar
            var assignments = new List<SIS.BLL.Services.DTOs.Assignment.AssignmentDto>();
            try { assignments = (await _assignmentApi.GetByGroupAsync(enrollment.GroupId)).ToList(); } catch { }

            var materials = new List<SIS.BLL.Services.DTOs.Material.CourseMaterialDto>();
            try { materials = (await _materialApi.GetVisibleByGroupAsync(enrollment.GroupId)).ToList(); } catch { }

            // Online dərslər — sessiya ID-sinə görə map (tələbənin qruplarından)
            var onlineLessonsBySession = new Dictionary<int, SIS.BLL.Services.DTOs.OnlineLesson.OnlineLessonDto>();
            try
            {
                var lessons = await _onlineLessonApi.GetForStudentAsync();
                foreach (var l in lessons.Where(x => x.GroupId == enrollment.GroupId))
                {
                    if (!onlineLessonsBySession.TryGetValue(l.ClassSessionId, out var existing) || l.Id > existing.Id)
                        onlineLessonsBySession[l.ClassSessionId] = l;
                }
            }
            catch { }
            ViewBag.OnlineLessons = onlineLessonsBySession;

            // F12 — akademik header üçün xülasə (season, tarixlər, official grade, GPA, certificate).
            SIS.BLL.Services.DTOs.Transcript.TranscriptDto? transcript = null;
            try { transcript = await _transcriptApi.GetAsync(id); } catch { }
            ViewBag.Transcript = transcript;

            ViewBag.Materials = materials;
            ViewBag.Enrollment = enrollment;
            ViewBag.Attendance = attendance;
            ViewBag.Summary = summary;
            ViewBag.AssessmentSubs = assessmentSubs;
            ViewBag.AssignmentSubs = assignmentSubs;
            ViewBag.Sessions = sessions;
            ViewBag.Announcements = announcements;
            ViewBag.Assignments = assignments;
            ViewBag.Today = _clock.Today;
            ViewBag.ActiveTab = tab;
            return View();
        }

        // Online dərsə qoşulma — JSON qaytarır (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinOnlineLesson(int lessonId)
        {
            try
            {
                var result = await _onlineLessonApi.JoinAsync(lessonId);
                return Json(new
                {
                    canJoin = result.CanJoin,
                    classroomUrl = result.ClassroomUrl,
                    meetingUrl = result.MeetingUrl,
                    message = result.Message
                });
            }
            catch (Refit.ApiException ex)
            {
                return Json(new { canJoin = false, message = string.IsNullOrWhiteSpace(ex.Content) ? ex.Message : ex.Content });
            }
            catch (Exception ex)
            {
                return Json(new { canJoin = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Transcript(int id, string? lang = "az")
        {
            try
            {
                var response = await _transcriptApi.GetPdfAsync(id, lang);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Transkript yüklənə bilmədi: {response.StatusCode}";
                    return RedirectToAction("Detail", new { id });
                }
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "transcript.pdf";
                return File(bytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Transkript xətası: {ex.Message}";
                return RedirectToAction("Detail", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTranscriptEmail(int id, string? lang = "az")
        {
            try
            {
                var response = await _transcriptApi.SendByEmailAsync(id, lang);
                if (response.IsSuccessStatusCode)
                    TempData["Success"] = "Transkript email-ə göndərildi";
                else
                    TempData["Error"] = "Transkript email göndərilə bilmədi";
            }
            catch (Refit.ApiException ex) { TempData["Error"] = string.IsNullOrEmpty(ex.Content) ? ex.Message : ex.Content; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Detail", new { id });
        }
    }
}
