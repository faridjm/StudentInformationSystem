using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.AdminAccess;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminAccessController : ControllerBase
    {
        private readonly IAdminAccessService _service;
        private readonly ICurrentUser _currentUser;

        public AdminAccessController(IAdminAccessService service, ICurrentUser currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>Cari istifadəçinin Admin panel girişi var? (özünü yoxlayır)</summary>
        [HttpGet("has-access")]
        public async Task<IActionResult> HasAccess()
        {
            var userId = _currentUser.UserId;
            if (string.IsNullOrEmpty(userId)) return Ok(false);

            // SuperAdmin həmişə icazəlidir
            if (_currentUser.IsSuperAdmin()) return Ok(true);

            return Ok(await _service.HasActiveAdminAccessAsync(userId));
        }

        [HttpGet("users")]
        [Authorize(Roles = RoleConstants.SuperAdmin)]
        public async Task<IActionResult> GetUsers()
            => Ok(await _service.GetAdminAccessUsersAsync());

        [HttpPost("activate")]
        [Authorize(Roles = RoleConstants.SuperAdmin)]
        public async Task<IActionResult> Activate([FromBody] ActivateAdminAccessDto dto)
        {
            var superAdminId = _currentUser.UserId!;
            await _service.ActivateAsync(dto.AdminUserId, dto.ExpiresAt, dto.Reason, superAdminId);
            return Ok();
        }

        [HttpPost("deactivate")]
        [Authorize(Roles = RoleConstants.SuperAdmin)]
        public async Task<IActionResult> Deactivate([FromBody] DeactivateAdminAccessDto dto)
        {
            var superAdminId = _currentUser.UserId!;
            await _service.DeactivateAsync(dto.AdminUserId, dto.Reason, superAdminId);
            return Ok();
        }
    }
}
