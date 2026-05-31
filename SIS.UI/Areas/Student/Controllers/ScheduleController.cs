using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.Common.Time;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Policy = "StudentOnly")]
    public class ScheduleController : Controller
    {
        private readonly IEnrollmentApi _enrollmentApi;
        private readonly IClassSessionApi _sessionApi;
        private readonly IAssessmentApi _assessmentApi;
        private readonly IClock _clock;

        public ScheduleController(IEnrollmentApi enrollmentApi, IClassSessionApi sessionApi,
            IAssessmentApi assessmentApi, IClock clock)
        {
            _enrollmentApi = enrollmentApi;
            _sessionApi = sessionApi;
            _assessmentApi = assessmentApi;
            _clock = clock;
        }

        public async Task<IActionResult> Index(DateOnly? from = null, DateOnly? to = null)
        {
            var today = _clock.Today;
            var fromDate = from ?? today.AddDays(-7);
            var toDate = to ?? today.AddDays(60);

            if (fromDate > toDate) (fromDate, toDate) = (toDate, fromDate);
            if ((toDate.DayNumber - fromDate.DayNumber) > 365)
                toDate = fromDate.AddDays(365);

            var enrollments = (await _enrollmentApi.GetMyEnrollmentsAsync()).ToList();

            var allSessions = new List<SIS.BLL.Services.DTOs.ClassSession.ClassSessionDto>();
            var allAssessments = new List<SIS.BLL.Services.DTOs.Assessment.AssessmentItemDto>();

            foreach (var e in enrollments)
            {
                try
                {
                    var sessions = await _sessionApi.GetByGroupAsync(e.GroupId);
                    allSessions.AddRange(sessions.Where(s => s.Date >= fromDate && s.Date <= toDate));

                    var assessments = await _assessmentApi.GetByGroupAsync(e.GroupId);
                    allAssessments.AddRange(assessments.Where(a =>
                        DateOnly.FromDateTime(a.Date) >= fromDate &&
                        DateOnly.FromDateTime(a.Date) <= toDate));
                }
                catch { }
            }

            ViewBag.From = fromDate;
            ViewBag.To = toDate;
            ViewBag.Today = today;
            ViewBag.Sessions = allSessions.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToList();
            ViewBag.Assessments = allAssessments;
            return View();
        }
    }
}
