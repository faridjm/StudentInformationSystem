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
        public ClassSessionController(IClassSessionService service) { _service = service; }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroup(int groupId)
            => Ok(await _service.GetByGroupAsync(groupId));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _service.GetByIdAsync(id);
            return s == null ? NotFound() : Ok(s);
        }

        [HttpPut("{id}/topic")]
        [Authorize(Roles = $"{RoleConstants.Teacher},{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> UpdateTopic(int id, [FromBody] UpdateTopicDto dto)
        {
            await _service.UpdateTopicAsync(id, dto.Topic ?? "");
            return Ok();
        }

        [HttpPut("{id}/complete")]
        [Authorize(Roles = $"{RoleConstants.Teacher},{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> CompleteSession(int id)
        {
            await _service.CompleteSessionAsync(id);
            return Ok(new { Message = "Sessiya tamamlandı" });
        }
    }
}
