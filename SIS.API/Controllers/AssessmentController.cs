using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Assessment;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _service;
        public AssessmentController(IAssessmentService service) { _service = service; }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroup(int groupId)
            => Ok(await _service.GetByGroupAsync(groupId));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] CreateAssessmentItemDto dto)
            => Ok(await _service.CreateAsync(dto));

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Update([FromBody] UpdateAssessmentItemDto dto)
        {
            await _service.UpdateAsync(dto);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }

        [HttpGet("{id}/submissions")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> GetSubmissions(int id)
            => Ok(await _service.GetSubmissionsAsync(id));

        [HttpGet("my-submissions/{enrollmentId}")]
        public async Task<IActionResult> GetMySubmissions(int enrollmentId)
            => Ok(await _service.GetMySubmissionsAsync(enrollmentId));

        [HttpPost("grade")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Grade([FromBody] GradeAssessmentDto dto)
        {
            await _service.GradeAsync(dto);
            return Ok();
        }
    }
}
