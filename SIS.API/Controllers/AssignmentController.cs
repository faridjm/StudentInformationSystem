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
        public AssignmentController(IAssignmentService service) { _service = service; }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroup(int groupId)
            => Ok(await _service.GetByGroupAsync(groupId));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var a = await _service.GetByIdAsync(id);
            return a == null ? NotFound() : Ok(a);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Create([FromBody] CreateAssignmentDto dto)
            => Ok(await _service.CreateAsync(dto));

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Update([FromBody] UpdateAssignmentDto dto)
        {
            await _service.UpdateAsync(dto);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }

        [HttpGet("{id}/submissions")]
        public async Task<IActionResult> GetSubmissions(int id)
            => Ok(await _service.GetSubmissionsAsync(id));

        [HttpGet("my-submissions/{enrollmentId}")]
        public async Task<IActionResult> GetMySubmissions(int enrollmentId)
            => Ok(await _service.GetMySubmissionsAsync(enrollmentId));

        [HttpPost("submit")]
        [Authorize(Roles = RoleConstants.Student)]
        public async Task<IActionResult> Submit([FromBody] CreateSubmissionDto dto)
            => Ok(await _service.SubmitAsync(dto));

        [HttpPost("grade")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> Grade([FromBody] GradeSubmissionDto dto)
        {
            await _service.GradeAsync(dto);
            return Ok();
        }
    }
}
