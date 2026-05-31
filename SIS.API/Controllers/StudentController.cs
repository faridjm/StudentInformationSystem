using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Student;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using System.Security.Claims;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _service;
        public StudentController(IStudentService service) { _service = service; }

        [HttpGet]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllStudentsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _service.GetStudentByIdAsync(id);
            return s == null ? NotFound() : Ok(s);
        }

        [HttpGet("me")]
        [Authorize(Roles = RoleConstants.Student)]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var s = await _service.GetStudentByUserIdAsync(userId!);
            return s == null ? NotFound() : Ok(s);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
        {
            await _service.CreateStudentAsync(dto);
            return Ok(new { Message = "Tələbə yaradıldı" });
        }

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Update([FromBody] UpdateStudentDto dto)
        {
            await _service.UpdateStudentAsync(dto);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteStudentAsync(id);
            return Ok();
        }
    }
}
