using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.WeeklySchedule;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WeeklyScheduleController : ControllerBase
    {
        private readonly IWeeklySchedulePatternService _service;

        public WeeklyScheduleController(IWeeklySchedulePatternService service)
        {
            _service = service;
        }

        [HttpGet("course/{courseOfferingId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetByCourse(int courseOfferingId)
        {
            var items = await _service.GetByCourseOfferingAsync(courseOfferingId);
            return Ok(items);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Set([FromBody] SetWeeklyScheduleDto dto)
        {
            await _service.SetScheduleAsync(dto);
            return Ok(new { Message = "Həftəlik cədvəl yeniləndi" });
        }

        [HttpDelete("course/{courseOfferingId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Clear(int courseOfferingId)
        {
            await _service.ClearScheduleAsync(courseOfferingId);
            return Ok(new { Message = "Həftəlik cədvəl silindi" });
        }
    }
}
