using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.Common.Time;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class DashboardController : Controller
    {
        private readonly IGroupApi _groupApi;
        private readonly IClassSessionApi _sessionApi;
        private readonly IAssessmentApi _assessmentApi;
        private readonly IEnrollmentApi _enrollmentApi;
        private readonly IAttendanceApi _attendanceApi;
        private readonly IClock _clock;

        public DashboardController(IGroupApi groupApi, IClassSessionApi sessionApi,
            IAssessmentApi assessmentApi, IEnrollmentApi enrollmentApi,
            IAttendanceApi attendanceApi, IClock clock)
        {
            _groupApi = groupApi; _sessionApi = sessionApi;
            _assessmentApi = assessmentApi; _enrollmentApi = enrollmentApi;
            _attendanceApi = attendanceApi; _clock = clock;
        }

        public async Task<IActionResult> Index()
        {
            var today = _clock.Today;
            var groups = (await _groupApi.GetMyGroupsAsync()).ToList();

            var todaySessions = new List<TodaySessionVm>();
            foreach (var g in groups)
            {
                var sessions = await _sessionApi.GetByGroupAsync(g.Id);
                var todaySession = sessions.FirstOrDefault(s => s.Date == today);
                if (todaySession == null) continue;

                var assessments = (await _assessmentApi.GetByGroupAsync(g.Id))
                    .Where(a => DateOnly.FromDateTime(a.Date) == today).ToList();

                var enrollments = (await _enrollmentApi.GetByGroupAsync(g.Id)).ToList();

                var existingAttendance = new List<SIS.BLL.Services.DTOs.Attendance.AttendanceDto>();
                try { existingAttendance = (await _attendanceApi.GetBySessionAsync(todaySession.Id)).ToList(); }
                catch { }

                todaySessions.Add(new TodaySessionVm
                {
                    GroupId = g.Id, GroupName = g.Name, CourseName = g.CourseName,
                    Session = todaySession, TodayAssessments = assessments,
                    Enrollments = enrollments, ExistingAttendance = existingAttendance,
                    AttendanceTaken = existingAttendance.Any()
                });
            }

            ViewBag.Today = today;
            ViewBag.Groups = groups;
            ViewBag.TodaySessions = todaySessions;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttendance(int sessionId,
            List<int> enrollmentIds, List<int> statuses)
        {
            var dtos = enrollmentIds.Select((eid, i) =>
                new SIS.BLL.Services.DTOs.Attendance.CreateAttendanceDto
                {
                    EnrollmentId = eid,
                    ClassSessionId = sessionId,
                    SessionDate = DateTime.UtcNow,
                    Status = (SIS.Entities.Enums.AttendanceStatus)statuses[i]
                }).ToList();

            await _attendanceApi.RecordBulkAsync(dtos);
            TempData["Success"] = "Davamiyyət qeyd edildi";
            return RedirectToAction("Index");
        }
    }

    public class TodaySessionVm
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public SIS.BLL.Services.DTOs.ClassSession.ClassSessionDto Session { get; set; } = null!;
        public List<SIS.BLL.Services.DTOs.Assessment.AssessmentItemDto> TodayAssessments { get; set; } = new();
        public List<SIS.BLL.Services.DTOs.Enrollment.EnrollmentListDto> Enrollments { get; set; } = new();
        public List<SIS.BLL.Services.DTOs.Attendance.AttendanceDto> ExistingAttendance { get; set; } = new();
        public bool AttendanceTaken { get; set; }
    }
}
