using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Grade;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GradeController : ControllerBase
    {
        private readonly IGradeService _gradeService;
        private readonly IStudentService _studentService;
        private readonly IAuthorizationService _authorizationService;

        public GradeController(
            IGradeService gradeService,
            IStudentService studentService,
            IAuthorizationService authorizationService)
        {
            _gradeService = gradeService;
            _studentService = studentService;
            _authorizationService = authorizationService;
        }

        [HttpGet("enrollment/{enrollmentId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetByEnrollment(int enrollmentId)
        {
            var auth = await _authorizationService.AuthorizeAsync(User, enrollmentId, "EnrollmentOwner");
            if (!auth.Succeeded) return Forbid();

            var grades = await _gradeService.GetGradesByEnrollmentAsync(enrollmentId);
            return Ok(grades);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> GetByStudent(int studentId)
        {
            var grades = await _gradeService.GetGradesByStudentAsync(studentId);
            return Ok(grades);
        }

        [HttpGet("my-grades")]
        [Authorize(Roles = RoleConstants.Student)]
        public async Task<IActionResult> GetMyGrades()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var student = await _studentService.GetStudentByUserIdAsync(userId!);
            if (student == null) return NotFound();
            var grades = await _gradeService.GetGradesByStudentAsync(student.Id);
            return Ok(grades);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Add([FromBody] CreateGradeDto dto)
        {
            var auth = await _authorizationService.AuthorizeAsync(User, dto.EnrollmentId, "GradeAuth");
            if (!auth.Succeeded) return Forbid();

            await _gradeService.AddGradeAsync(dto);
            return Ok(new { Message = "Qiymət uğurla əlavə edildi" });
        }

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Update([FromBody] UpdateGradeDto dto)
        {
            var auth = await _authorizationService.AuthorizeAsync(User, dto.Id, "GradeAuth");
            if (!auth.Succeeded) return Forbid();

            await _gradeService.UpdateGradeAsync(dto);
            return Ok(new { Message = "Qiymət uğurla yeniləndi" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _gradeService.DeleteGradeAsync(id);
            return Ok(new { Message = "Qiymət uğurla silindi" });
        }
    }
}
