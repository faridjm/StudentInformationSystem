using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.AuditLog;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // bütün autentikasiya olunmuş istifadəçilər (action-larda role gates)
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _service;
        public AuditLogController(IAuditLogService service) { _service = service; }

        [HttpGet]
        [Authorize(Roles = RoleConstants.SuperAdmin)]
        public async Task<IActionResult> Get(
            [FromQuery] string? action,
            [FromQuery] string? entityName,
            [FromQuery] string? search,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetPagedAsync(new AuditLogFilterDto
            {
                Action = action,
                EntityName = entityName,
                Search = search,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            });
            return Ok(result);
        }

        [HttpGet("actions")]
        [Authorize(Roles = RoleConstants.SuperAdmin)]
        public async Task<IActionResult> GetActions() => Ok(await _service.GetActionsAsync());

        [HttpGet("entities")]
        [Authorize(Roles = RoleConstants.SuperAdmin)]
        public async Task<IActionResult> GetEntities() => Ok(await _service.GetEntitiesAsync());

        /// <summary>
        /// UI tərəfindən qeydə alınan təhlükəsizlik hadisəsi (whitelist əsaslı).
        /// Hər autentikasiya olunmuş istifadəçi öz hadisəsini logla bilər
        /// (məsələn: AccessDenied panel keçidi).
        /// </summary>
        [HttpPost("security-event")]
        public async Task<IActionResult> SecurityEvent([FromBody] SecurityEventDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Action))
                return BadRequest();

            // Yalnız aşağıdakı təhlükəsizlik hadisələri qəbul olunur (whitelist).
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "AccessDeniedToPublicAdmin",
                "AccessDeniedToAcademicAdmin",
                "AdminAccessDeniedInactive"
            };
            if (!allowed.Contains(dto.Action))
                return BadRequest(new { Message = "Action whitelist-də deyil" });

            await _service.LogAsync(dto.Action, "Security", null, dto.Description);
            return Ok();
        }
    }
}
