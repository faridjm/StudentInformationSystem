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
        private readonly IStudentApi _studentApi;
        private readonly IClassSessionApi _sessionApi;
        private readonly IClock _clock;

        public ScheduleController(IStudentApi studentApi, IClassSessionApi sessionApi, IClock clock)
        {
            _studentApi = studentApi;
            _sessionApi = sessionApi;
            _clock = clock;
        }

        // GET: /Student/Schedule?from=2026-09-01&to=2026-10-31
        public async Task<IActionResult> Index(DateOnly? from = null, DateOnly? to = null)
        {
            var today = _clock.Today;
            var fromDate = from ?? today.AddDays(-7);
            var toDate = to ?? today.AddDays(30);

            // from > to olarsa düzəlt
            if (fromDate > toDate)
                (fromDate, toDate) = (toDate, fromDate);

            // Sui-istifadədən qorunma üçün ən çox 2 il
            if ((toDate.DayNumber - fromDate.DayNumber) > 730)
                toDate = fromDate.AddDays(730);

            var student = await _studentApi.GetMeAsync();
            var sessions = await _sessionApi.GetByStudentRangeAsync(student.Id, fromDate, toDate);

            ViewBag.Student = student;
            ViewBag.From = fromDate;
            ViewBag.To = toDate;
            ViewBag.Today = today;
            ViewBag.Sessions = sessions
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToList();

            return View();
        }
    }
}
