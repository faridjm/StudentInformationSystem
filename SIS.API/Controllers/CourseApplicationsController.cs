using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Public;
using SIS.BLL.Services.Interfaces;

namespace SIS.API.Controllers
{
    /// <summary>
    /// Public saytdan gələn müraciətlərin admin idarəetməsi.
    /// Yalnız Admin/SuperAdmin/Moderator rolları üçündür.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin,Moderator")]
    public class CourseApplicationsController : ControllerBase
    {
        private readonly ICourseApplicationService _service;
        public CourseApplicationsController(ICourseApplicationService service) { _service = service; }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] CourseApplicationFilterDto filter)
            => Ok(await _service.GetPagedAsync(filter ?? new CourseApplicationFilterDto()));

        [HttpGet("stats")]
        public async Task<IActionResult> Stats() => Ok(await _service.GetStatsAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var dto = await _service.GetDetailAsync(id);
            return dto == null ? NotFound() : Ok(dto);
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateApplicationStatusDto dto)
        {
            try { await _service.UpdateStatusAsync(id, dto); return NoContent(); }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPut("{id:int}/note")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] UpdateApplicationNoteDto dto)
        {
            try { await _service.UpdateAdminNoteAsync(id, dto); return NoContent(); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost("{id:int}/convert")]
        public async Task<IActionResult> ConvertToStudent(int id, [FromBody] ConvertApplicationToStudentDto dto)
        {
            try { return Ok(await _service.ConvertToStudentAsync(id, dto)); }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        }
    }
}
