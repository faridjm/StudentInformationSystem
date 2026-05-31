using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Teacher;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using System.Security.Claims;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _service;
        public TeacherController(ITeacherService service) { _service = service; }

        [HttpGet]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllTeachersAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var t = await _service.GetTeacherByIdAsync(id);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpGet("me")]
        [Authorize(Roles = RoleConstants.Teacher)]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var t = await _service.GetTeacherByUserIdAsync(userId!);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        {
            await _service.CreateTeacherAsync(dto);
            return Ok(new { Message = "Müəllim yaradıldı" });
        }

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Update([FromBody] UpdateTeacherDto dto)
        {
            await _service.UpdateTeacherAsync(dto);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteTeacherAsync(id);
            return Ok();
        }
    }
}
