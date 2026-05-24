using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Attendance;
using SIS.BLL.Services.DTOs.Grade;
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

        public CoursesController(
            ITeacherApi teacherApi,
            ICourseOfferingApi offeringApi,
            IEnrollmentApi enrollmentApi,
            IGradeApi gradeApi,
            IAttendanceApi attendanceApi)
        {
            _teacherApi = teacherApi;
            _offeringApi = offeringApi;
            _enrollmentApi = enrollmentApi;
            _gradeApi = gradeApi;
            _attendanceApi = attendanceApi;
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

        public async Task<IActionResult> Attendance(int id)
        {
            var offering = await _offeringApi.GetByIdAsync(id);
            if (offering == null) return NotFound();

            var enrollments = await _enrollmentApi.GetByCourseAsync(id);
            ViewBag.Offering = offering;
            ViewBag.Enrollments = enrollments;
            ViewBag.Today = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecordAttendance(
            int courseOfferingId,
            List<int> enrollmentIds,
            List<int> statuses,
            string sessionDate)
        {
            try
            {
                var dtos = enrollmentIds.Select((enrollmentId, i) =>
                    new CreateAttendanceDto
                    {
                        EnrollmentId = enrollmentId,
                        SessionDate = DateTime.Parse(sessionDate),
                        Status = (SIS.Entities.Enums.AttendanceStatus)statuses[i]
                    }).ToList();

                await _attendanceApi.RecordBulkAsync(dtos);
                TempData["Success"] = "Davamiyyət uğurla qeyd edildi";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Attendance", new { id = courseOfferingId });
        }
    }
}
