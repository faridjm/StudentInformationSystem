using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.SystemUsers;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    /// <summary>
    /// SuperAdmin-only sistem istifadəçi idarəetməsi.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public class SystemUsersController : ControllerBase
    {
        private readonly ISystemUsersService _service;
        private readonly ICurrentUser _currentUser;

        public SystemUsersController(ISystemUsersService service, ICurrentUser currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var res = await _service.GetPagedAsync(new SystemUserFilterDto
            {
                Search = search,
                Role = role,
                Status = status,
                Page = page,
                PageSize = pageSize
            });
            return Ok(res);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
            => Ok(await _service.GetByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSystemUserDto dto)
        {
            var newId = await _service.CreateAsync(dto, _currentUser.UserId!);
            return Ok(new { id = newId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateSystemUserDto dto)
        {
            await _service.UpdateAsync(id, dto, _currentUser.UserId!);
            return Ok();
        }

        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangeSystemUserPasswordDto dto)
        {
            await _service.ChangePasswordAsync(id, dto, _currentUser.UserId!);
            return Ok();
        }

        [HttpPost("{id}/enable")]
        public async Task<IActionResult> Enable(string id)
        {
            await _service.SetStatusAsync(id, true, _currentUser.UserId!);
            return Ok();
        }

        [HttpPost("{id}/disable")]
        public async Task<IActionResult> Disable(string id)
        {
            await _service.SetStatusAsync(id, false, _currentUser.UserId!);
            return Ok();
        }
    }
}
