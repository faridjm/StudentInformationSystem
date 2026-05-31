using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Enrollment;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
// ICurrentUser, IStudentService from SIS.BLL

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _service;
        private readonly IStudentService _studentService;
        private readonly ICurrentUser _currentUser;

        public EnrollmentController(IEnrollmentService service, IStudentService studentService, ICurrentUser currentUser)
        {
            _service = service;
            _studentService = studentService;
            _currentUser = currentUser;
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroup(int groupId)
            => Ok(await _service.GetByGroupAsync(groupId));

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(int studentId)
            => Ok(await _service.GetByStudentAsync(studentId));

        [HttpGet("my")]
        [Authorize(Roles = RoleConstants.Student)]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var userId = _currentUser.UserId;
            if (userId == null) return Unauthorized();
            var student = await _studentService.GetStudentByUserIdAsync(userId);
            if (student == null) return NotFound();
            return Ok(await _service.GetByStudentAsync(student.Id));
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Enroll([FromBody] CreateEnrollmentDto dto)
        {
            await _service.EnrollAsync(dto);
            return Ok(new { Message = "Qeydiyyat tamamlandı" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Drop(int id)
        {
            await _service.DropAsync(id);
            return Ok(new { Message = "Qeydiyyat ləğv edildi" });
        }
    }
}
