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
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpGet("enrollment/{enrollmentId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetByEnrollment(int enrollmentId)
        {
            var records = await _attendanceService.GetAttendanceByEnrollmentAsync(enrollmentId);
            return Ok(records);
        }

        [HttpGet("course/{courseOfferingId}/date/{date}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> GetByDate(int courseOfferingId, DateTime date)
        {
            var records = await _attendanceService.GetAttendanceByDateAsync(courseOfferingId, date);
            return Ok(records);
        }

        [HttpGet("enrollment/{enrollmentId}/summary")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetSummary(int enrollmentId)
        {
            var summary = await _attendanceService.GetAttendanceSummaryAsync(enrollmentId);
            return Ok(summary);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Record([FromBody] CreateAttendanceDto dto)
        {
            await _attendanceService.RecordAttendanceAsync(dto);
            return Ok(new { Message = "Davamiyyət uğurla qeyd edildi" });
        }

        [HttpPost("bulk")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> RecordBulk([FromBody] IEnumerable<CreateAttendanceDto> dtos)
        {
            await _attendanceService.RecordBulkAttendanceAsync(dtos);
            return Ok(new { Message = "Davamiyyət uğurla qeyd edildi" });
        }
    }
}
