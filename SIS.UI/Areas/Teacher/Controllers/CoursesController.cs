using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Announcement;
using SIS.BLL.Services.DTOs.Attendance;
using SIS.BLL.Services.DTOs.Material;
using SIS.Common.Time;
using SIS.Entities.Enums;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class CoursesController : Controller
    {
        private readonly IGroupApi _groupApi;
        private readonly ITeacherApi _teacherApi;
        private readonly IEnrollmentApi _enrollmentApi;
        private readonly IClassSessionApi _sessionApi;
        private readonly IAssessmentApi _assessmentApi;
        private readonly IAnnouncementApi _announcementApi;
        private readonly IAssignmentApi _assignmentApi;
        private readonly IAttendanceApi _attendanceApi;
        private readonly IClock _clock;
        private readonly ITranscriptApi _transcriptApi;
        private readonly ICourseMaterialApi _materialApi;

        public CoursesController(IGroupApi groupApi, ITeacherApi teacherApi,
            IEnrollmentApi enrollmentApi, IClassSessionApi sessionApi,
            IAssessmentApi assessmentApi, IAnnouncementApi announcementApi,
            IAssignmentApi assignmentApi, IAttendanceApi attendanceApi,
            IClock clock, ITranscriptApi transcriptApi, ICourseMaterialApi materialApi)
        {
            _groupApi = groupApi;
            _teacherApi = teacherApi;
            _enrollmentApi = enrollmentApi;
            _sessionApi = sessionApi;
            _assessmentApi = assessmentApi;
            _announcementApi = announcementApi;
            _assignmentApi = assignmentApi;
            _attendanceApi = attendanceApi;
            _clock = clock;
            _transcriptApi = transcriptApi;
            _materialApi = materialApi;
        }

        public async Task<IActionResult> Index()
        {
            var groups = await _groupApi.GetMyGroupsAsync();
            return View(groups);
        }

        public async Task<IActionResult> Detail(int id, string tab = "overview")
        {
            var group = await _groupApi.GetByIdAsync(id);
            var enrollments = (await _enrollmentApi.GetByGroupAsync(id)).ToList();
            var sessions = (await _sessionApi.GetByGroupAsync(id)).OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToList();
            var assessments = (await _assessmentApi.GetByGroupAsync(id)).OrderBy(a => a.Date).ToList();

            var announcements = new List<AnnouncementDto>();
            try { announcements = (await _announcementApi.GetByGroupAsync(id)).ToList(); } catch { }

            var assignments = new List<SIS.BLL.Services.DTOs.Assignment.AssignmentDto>();
            try { assignments = (await _assignmentApi.GetByGroupAsync(id)).ToList(); } catch { }

            var materials = new List<SIS.BLL.Services.DTOs.Material.CourseMaterialDto>();
            try { materials = (await _materialApi.GetByGroupAsync(id)).ToList(); } catch { }

            // Davamiyyət: hər enrollment üçün summary
            var attendanceSummaries = new Dictionary<int, AttendanceSummaryDto>();
            foreach (var e in enrollments)
            {
                try { attendanceSummaries[e.Id] = await _attendanceApi.GetSummaryAsync(e.Id); }
                catch { attendanceSummaries[e.Id] = new AttendanceSummaryDto(); }
            }

            // F6 — hər enrollment üçün akademik xülasə (grade %, GPA, missing assignments, risk).
            // Data transcript servisindən gəlir; hesablanmırsa o tələbə üçün sadəcə yox sayılır.
            var transcripts = new Dictionary<int, SIS.BLL.Services.DTOs.Transcript.TranscriptDto>();
            foreach (var e in enrollments)
            {
                try { transcripts[e.Id] = await _transcriptApi.GetAsync(e.Id); }
                catch { }
            }
            ViewBag.Transcripts = transcripts;

            // Hər sessiya üçün mövcud davamiyyət records
            var attendancePerSession = new Dictionary<int, List<AttendanceDto>>();
            var pastSessions = sessions.Where(s => s.Date <= _clock.Today).ToList();
            foreach (var s in pastSessions)
            {
                try { attendancePerSession[s.Id] = (await _attendanceApi.GetBySessionAsync(s.Id)).ToList(); }
                catch { attendancePerSession[s.Id] = new List<AttendanceDto>(); }
            }

            ViewBag.Enrollments = enrollments;
            ViewBag.Sessions = sessions;
            ViewBag.Assessments = assessments;
            ViewBag.Announcements = announcements;
            ViewBag.Assignments = assignments;
            ViewBag.Materials = materials;
            ViewBag.AttendanceSummaries = attendanceSummaries;
            ViewBag.AttendancePerSession = attendancePerSession;
            ViewBag.Today = _clock.Today;
            ViewBag.ActiveTab = tab;
            return View(group);
        }

        // Davamiyyət saxla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance(int groupId, int sessionId,
            List<int> enrollmentIds, List<int> statuses)
        {
            var dtos = enrollmentIds.Select((eid, i) =>
                new CreateAttendanceDto
                {
                    EnrollmentId = eid,
                    ClassSessionId = sessionId,
                    SessionDate = DateTime.UtcNow,
                    Status = (SIS.Entities.Enums.AttendanceStatus)statuses[i]
                }).ToList();

            await _attendanceApi.RecordBulkAsync(dtos);
            TempData["Success"] = "Davamiyyət qeyd edildi";
            return RedirectToAction("Detail", new { id = groupId, tab = "attendance" });
        }

        // Sessiyanı tamamla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteSession(int groupId, int sessionId)
        {
            await _sessionApi.CompleteSessionAsync(sessionId);
            TempData["Success"] = "Dərs tamamlandı";
            return RedirectToAction("Detail", new { id = groupId, tab = "schedule" });
        }

        // Elan yarat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(int groupId, string title, string content)
        {
            var dto = new CreateAnnouncementDto
            {
                Title = title,
                Content = content,
                GroupId = groupId,
                TargetType = SIS.Entities.Enums.AnnouncementTarget.Course,
                IsGlobal = false
            };
            await _announcementApi.CreateAsync(dto);
            TempData["Success"] = "Elan yayımlandı";
            return RedirectToAction("Detail", new { id = groupId, tab = "announcements" });
        }

        // ── Materiallar ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMaterial(int groupId, string Title, string? Description,
            int MaterialType, string? LinkUrl, string? FilePath, string? FileName, long? FileSize)
        {
            try
            {
                await _materialApi.CreateAsync(new CreateCourseMaterialDto
                {
                    GroupId = groupId,
                    Title = Title,
                    Description = Description,
                    MaterialType = (SIS.Entities.Enums.MaterialType)MaterialType,
                    LinkUrl = LinkUrl,
                    FilePath = FilePath,
                    FileName = FileName,
                    FileSize = FileSize
                });
                TempData["Success"] = "Material əlavə edildi";
            }
            catch { TempData["Error"] = "Material əlavə edilə bilmədi (məlumatları yoxlayın)"; }
            return RedirectToAction("Detail", new { id = groupId, tab = "materials" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMaterial(int id, int groupId, string Title, string? Description,
            int MaterialType, string? LinkUrl, string? FilePath, string? FileName, long? FileSize)
        {
            try
            {
                await _materialApi.UpdateAsync(new UpdateCourseMaterialDto
                {
                    Id = id,
                    Title = Title,
                    Description = Description,
                    MaterialType = (SIS.Entities.Enums.MaterialType)MaterialType,
                    LinkUrl = LinkUrl,
                    FilePath = FilePath,
                    FileName = FileName,
                    FileSize = FileSize
                });
                TempData["Success"] = "Material yeniləndi";
            }
            catch { TempData["Error"] = "Material yenilənə bilmədi"; }
            return RedirectToAction("Detail", new { id = groupId, tab = "materials" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleMaterial(int id, int groupId, bool active)
        {
            try { await _materialApi.SetActiveAsync(id, active); TempData["Success"] = active ? "Material göstərilir" : "Material gizlədildi"; }
            catch { TempData["Error"] = "Əməliyyat alınmadı"; }
            return RedirectToAction("Detail", new { id = groupId, tab = "materials" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMaterial(int id, int groupId)
        {
            try { await _materialApi.DeleteAsync(id); TempData["Success"] = "Material silindi"; }
            catch { TempData["Error"] = "Material silinə bilmədi"; }
            return RedirectToAction("Detail", new { id = groupId, tab = "materials" });
        }

        // Tələbə transkriptini yüklə
        public async Task<IActionResult> Transcript(int enrollmentId)
        {
            var response = await _transcriptApi.GetPdfAsync(enrollmentId);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Transkript yüklənə bilmədi";
                return RedirectToAction("Index");
            }
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "transcript.pdf";
            return File(bytes, "application/pdf", fileName);
        }
    }
}
