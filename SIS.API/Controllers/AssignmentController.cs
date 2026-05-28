using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Assignment;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _service;

        public AssignmentController(IAssignmentService service)
        {
            _service = service;
        }

        [HttpGet("course/{courseOfferingId}")]
        public async Task<IActionResult> GetByCourse(int courseOfferingId)
        {
            var items = await _service.GetByCourseOfferingAsync(courseOfferingId);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Create([FromBody] CreateAssignmentDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Update([FromBody] UpdateAssignmentDto dto)
        {
            await _service.UpdateAsync(dto);
            return Ok(new { Message = "Tapşırıq yeniləndi" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new { Message = "Tapşırıq silindi" });
        }

        // ── Submissions ──────────────────────────────────────────────

        [HttpGet("{id}/submissions")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> GetSubmissions(int id)
        {
            var items = await _service.GetSubmissionsAsync(id);
            return Ok(items);
        }

        [HttpGet("{assignmentId}/submission/{enrollmentId}")]
        public async Task<IActionResult> GetSubmission(int assignmentId, int enrollmentId)
        {
            var item = await _service.GetSubmissionAsync(assignmentId, enrollmentId);
            return Ok(item);
        }

        [HttpGet("enrollment/{enrollmentId}/submissions")]
        public async Task<IActionResult> GetMySubmissions(int enrollmentId)
        {
            var items = await _service.GetMySubmissionsAsync(enrollmentId);
            return Ok(items);
        }

        [HttpPost("submit")]
        [Authorize(Roles = $"{RoleConstants.Student}")]
        public async Task<IActionResult> Submit([FromBody] CreateSubmissionDto dto)
        {
            var result = await _service.SubmitAsync(dto);
            return Ok(result);
        }

        [HttpPost("grade")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Grade([FromBody] GradeSubmissionDto dto)
        {
            await _service.GradeAsync(dto);
            return Ok(new { Message = "Qiymət verildi" });
        }
    }
}
