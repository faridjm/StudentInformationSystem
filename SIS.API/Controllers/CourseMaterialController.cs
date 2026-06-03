using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Material;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CourseMaterialController : ControllerBase
    {
        private const string TeacherRoles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}";

        private readonly ICourseMaterialService _service;
        private readonly ICurrentUser _currentUser;
        private readonly IStudentService _studentService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly ITeacherService _teacherService;

        public CourseMaterialController(ICourseMaterialService service, ICurrentUser currentUser,
            IStudentService studentService, IEnrollmentService enrollmentService, ITeacherService teacherService)
        {
            _service = service;
            _currentUser = currentUser;
            _studentService = studentService;
            _enrollmentService = enrollmentService;
            _teacherService = teacherService;
        }

        [HttpGet("group/{groupId}")]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> GetByGroup(int groupId)
            => Ok(await _service.GetByGroupAsync(groupId));

        [HttpGet("group/{groupId}/visible")]
        public async Task<IActionResult> GetVisible(int groupId)
        {
            // Student yalnız enrolled olduğu qrupun materiallarını görə bilər
            if (_currentUser.IsStudent())
            {
                var student = await _studentService.GetStudentByUserIdAsync(_currentUser.UserId!);
                if (student == null) return Forbid();
                var enrollments = await _enrollmentService.GetByStudentAsync(student.Id);
                if (!enrollments.Any(e => e.GroupId == groupId)) return Forbid();
            }
            return Ok(await _service.GetVisibleByGroupAsync(groupId));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> GetById(int id)
        {
            var m = await _service.GetByIdAsync(id);
            return m == null ? NotFound() : Ok(m);
        }

        [HttpPost]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> Create([FromBody] CreateCourseMaterialDto dto)
        {
            int teacherId = 0;
            var teacher = await _teacherService.GetTeacherByUserIdAsync(_currentUser.UserId!);
            if (teacher != null) teacherId = teacher.Id;
            return Ok(await _service.CreateAsync(dto, teacherId));
        }

        [HttpPut]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> Update([FromBody] UpdateCourseMaterialDto dto)
        {
            await _service.UpdateAsync(dto);
            return Ok();
        }

        [HttpPost("{id}/active")]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> SetActive(int id, [FromQuery] bool active)
        {
            await _service.SetActiveAsync(id, active);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }
    }
}
