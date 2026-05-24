using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public CoursesController(
            IEnrollmentApi enrollmentApi,
            IGradeApi gradeApi,
            IAttendanceApi attendanceApi)
        {
            _enrollmentApi = enrollmentApi;
            _gradeApi = gradeApi;
            _attendanceApi = attendanceApi;
        }

        public async Task<IActionResult> Index()
        {
            var enrollments = await _enrollmentApi.GetMyEnrollmentsAsync();
            return View(enrollments);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var enrollment = await _enrollmentApi.GetByIdAsync(id);
            if (enrollment == null) return NotFound();

            var grades = await _gradeApi.GetByEnrollmentAsync(id);
            var attendance = await _attendanceApi.GetByEnrollmentAsync(id);
            var summary = await _attendanceApi.GetSummaryAsync(id);

            ViewBag.Enrollment = enrollment;
            ViewBag.Grades = grades;
            ViewBag.Attendance = attendance;
            ViewBag.Summary = summary;

            return View();
        }
    }
}
