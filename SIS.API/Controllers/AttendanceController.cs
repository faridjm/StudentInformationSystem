using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Attendance;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;
        public AttendanceController(IAttendanceService service) { _service = service; }

        [HttpGet("enrollment/{enrollmentId}")]
        public async Task<IActionResult> GetByEnrollment(int enrollmentId)
            => Ok(await _service.GetAttendanceByEnrollmentAsync(enrollmentId));

        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetBySession(int sessionId)
            => Ok(await _service.GetAttendanceBySessionAsync(sessionId));

        [HttpGet("enrollment/{enrollmentId}/summary")]
        public async Task<IActionResult> GetSummary(int enrollmentId)
            => Ok(await _service.GetAttendanceSummaryAsync(enrollmentId));

        [HttpPost("bulk")]
        [Authorize(Roles = $"{RoleConstants.Teacher},{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> RecordBulk([FromBody] IEnumerable<CreateAttendanceDto> dtos)
        {
            await _service.RecordBulkAttendanceAsync(dtos);
            return Ok(new { Message = "Davamiyyət qeyd edildi" });
        }

        /// <summary>
        /// Yalnız müəllim üçün demo davamiyyət generasiyası (təqdimat/test).
        /// Real istifadə üçün /bulk endpoint-i istifadə olunur.
        /// </summary>
        [HttpPost("demo")]
        [Authorize(Roles = RoleConstants.Teacher)]
        public async Task<IActionResult> GenerateDemo([FromBody] GenerateDemoAttendanceDto dto)
        {
            var teacherUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException();
            var result = await _service.GenerateTeacherDemoAttendanceAsync(dto, teacherUserId);
            return Ok(result);
        }
    }
}
