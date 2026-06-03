using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.AuditLog;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _service;
        public AuditLogController(IAuditLogService service) { _service = service; }

        [HttpGet]
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
        public async Task<IActionResult> GetActions() => Ok(await _service.GetActionsAsync());

        [HttpGet("entities")]
        public async Task<IActionResult> GetEntities() => Ok(await _service.GetEntitiesAsync());
    }
}
