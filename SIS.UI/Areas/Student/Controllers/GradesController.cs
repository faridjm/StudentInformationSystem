using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Policy = "StudentOnly")]
    public class GradesController : Controller
    {
        private readonly IGradeApi _gradeApi;
        private readonly IStudentApi _studentApi;

        public GradesController(IGradeApi gradeApi, IStudentApi studentApi)
        {
            _gradeApi = gradeApi;
            _studentApi = studentApi;
        }

        public async Task<IActionResult> Index()
        {
            var grades = await _gradeApi.GetMyGradesAsync();
            return View(grades);
        }
    }
}
