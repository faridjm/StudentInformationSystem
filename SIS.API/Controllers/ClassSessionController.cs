using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.ClassSession;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClassSessionController : ControllerBase
    {
        private readonly IClassSessionService _service;

        public ClassSessionController(IClassSessionService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("course/{courseOfferingId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetByCourse(int courseOfferingId)
        {
            var items = await _service.GetByCourseOfferingAsync(courseOfferingId);
            return Ok(items);
        }

        [HttpGet("course/{courseOfferingId}/range")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetByCourseRange(
            int courseOfferingId,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to)
        {
            var items = await _service.GetByCourseOfferingAndRangeAsync(courseOfferingId, from, to);
            return Ok(items);
        }

        [HttpGet("student/{studentId}/range")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetByStudentRange(
            int studentId,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to)
        {
            var items = await _service.GetByStudentAndRangeAsync(studentId, from, to);
            return Ok(items);
        }

        [HttpPost("generate/{courseOfferingId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Generate(int courseOfferingId)
        {
            var count = await _service.GenerateFromWeeklyScheduleAsync(courseOfferingId);
            return Ok(new { Message = $"{count} sessiya yaradıldı", Created = count });
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Create([FromBody] CreateClassSessionDto dto)
        {
            var id = await _service.CreateAsync(dto);
            return Ok(new { Message = "Sessiya yaradıldı", Id = id });
        }

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Update([FromBody] UpdateClassSessionDto dto)
        {
            await _service.UpdateAsync(dto);
            return Ok(new { Message = "Sessiya yeniləndi" });
        }

        [HttpPost("cancel")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Cancel([FromBody] CancelClassSessionDto dto)
        {
            await _service.CancelAsync(dto);
            return Ok(new { Message = "Sessiya ləğv olundu" });
        }

        [HttpPost("makeup")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> CreateMakeup([FromBody] CreateMakeupSessionDto dto)
        {
            var id = await _service.CreateMakeupAsync(dto);
            return Ok(new { Message = "Makeup sessiyası yaradıldı", Id = id });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new { Message = "Sessiya silindi" });
        }
    }
}
