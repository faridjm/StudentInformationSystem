using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IGroupApi _groupApi;

        public DashboardController(IStudentApi studentApi, ITeacherApi teacherApi,
            ICourseApi courseApi, IGroupApi groupApi)
        {
            _studentApi = studentApi;
            _teacherApi = teacherApi;
            _courseApi = courseApi;
            _groupApi = groupApi;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var students = await _studentApi.GetAllAsync();
                var teachers = await _teacherApi.GetAllAsync();
                var courses = await _courseApi.GetAllAsync();
                var groups = await _groupApi.GetAllAsync();

                ViewBag.StudentCount = students.Count();
                ViewBag.TeacherCount = teachers.Count();
                ViewBag.CourseCount = courses.Count();
                ViewBag.GroupCount = groups.Count();
            }
            catch { }
            return View();
        }
    }
}
