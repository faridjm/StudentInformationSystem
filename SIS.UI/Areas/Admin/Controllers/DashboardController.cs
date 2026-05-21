using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.Common.Pagination;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class DashboardController : Controller
    {
        private readonly IStudentApi _studentApi;
        private readonly ITeacherApi _teacherApi;
        private readonly ICourseApi _courseApi;
        private readonly IEnrollmentApi _enrollmentApi;

        public DashboardController(
            IStudentApi studentApi,
            ITeacherApi teacherApi,
            ICourseApi courseApi,
            IEnrollmentApi enrollmentApi)
        {
            _studentApi = studentApi;
            _teacherApi = teacherApi;
            _courseApi = courseApi;
            _enrollmentApi = enrollmentApi;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _studentApi.GetAllAsync(new PagedRequest { PageSize = 1 });
            var teachers = await _teacherApi.GetAllAsync(new PagedRequest { PageSize = 1 });
            var courses = await _courseApi.GetAllAsync(new PagedRequest { PageSize = 1 });
            var enrollments = await _enrollmentApi.GetAllAsync();

            ViewBag.StudentCount = students.TotalCount;
            ViewBag.TeacherCount = teachers.TotalCount;
            ViewBag.CourseCount = courses.TotalCount;
            ViewBag.EnrollmentCount = enrollments.Count();

            return View();
        }
    }
}
