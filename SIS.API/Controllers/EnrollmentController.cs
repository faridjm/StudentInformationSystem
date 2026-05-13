using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Enrollment;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IStudentService _studentService;
        private readonly IAuthorizationService _authorizationService;

        public EnrollmentController(
            IEnrollmentService enrollmentService,
            IStudentService studentService,
            IAuthorizationService authorizationService)
        {
            _enrollmentService = enrollmentService;
            _studentService = studentService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetAll()
        {
            var enrollments = await _enrollmentService.GetAllEnrollmentsAsync();
            return Ok(enrollments);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetById(int id)
        {
            var auth = await _authorizationService.AuthorizeAsync(User, id, "EnrollmentOwner");
            if (!auth.Succeeded) return Forbid();

            var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
            if (enrollment == null) return NotFound();
            return Ok(enrollment);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetByStudent(int studentId)
        {
            var auth = await _authorizationService.AuthorizeAsync(User, studentId, "StudentOwner");
            if (!auth.Succeeded) return Forbid();

            var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(studentId);
            return Ok(enrollments);
        }

        [HttpGet("my-enrollments")]
        [Authorize(Roles = RoleConstants.Student)]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var student = await _studentService.GetStudentByUserIdAsync(userId!);
            if (student == null) return NotFound();
            var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(student.Id);
            return Ok(enrollments);
        }

        [HttpGet("course/{courseOfferingId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> GetByCourse(int courseOfferingId)
        {
            var enrollments = await _enrollmentService.GetCourseEnrollmentsAsync(courseOfferingId);
            return Ok(enrollments);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Student}")]
        public async Task<IActionResult> Enroll([FromBody] CreateEnrollmentDto dto)
        {
            await _enrollmentService.EnrollStudentAsync(dto);
            return Ok(new { Message = "Qeydiyyat uğurla tamamlandı" });
        }

        [HttpPut("{id}/drop")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Student}")]
        public async Task<IActionResult> Drop(int id)
        {
            await _enrollmentService.DropEnrollmentAsync(id);
            return Ok(new { Message = "Kursdan uğurla çıxıldı" });
        }
    }
}
