using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Student;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using SIS.Common.Pagination;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IAuthorizationService _authorizationService;

        public StudentController(
            IStudentService studentService,
            IAuthorizationService authorizationService)
        {
            _studentService = studentService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var students = await _studentService.GetAllStudentsAsync(request);
            return Ok(students);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (User.IsInRole(RoleConstants.Student))
            {
                var auth = await _authorizationService.AuthorizeAsync(User, id, "StudentOwner");
                if (!auth.Succeeded) return Forbid();
            }

            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null) return NotFound();
            return Ok(student);
        }

        [HttpGet("me")]
        [Authorize(Roles = RoleConstants.Student)]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var student = await _studentService.GetStudentByUserIdAsync(userId!);
            if (student == null) return NotFound();
            return Ok(student);
        }

        [HttpGet("program/{programId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetByProgram(int programId)
        {
            var students = await _studentService.GetStudentsByProgramAsync(programId);
            return Ok(students);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
        {
            await _studentService.CreateStudentAsync(dto);
            return Ok(new { Message = "Tələbə uğurla yaradıldı" });
        }

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Update([FromBody] UpdateStudentDto dto)
        {
            await _studentService.UpdateStudentAsync(dto);
            return Ok(new { Message = "Tələbə uğurla yeniləndi" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _studentService.DeleteStudentAsync(id);
            return Ok(new { Message = "Tələbə uğurla silindi" });
        }
    }
}
