using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Archive;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using System.Security.Claims;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public class ArchiveController : ControllerBase
    {
        private readonly IArchiveService _service;
        public ArchiveController(IArchiveService service) { _service = service; }

        [HttpGet("chats")]
        public async Task<IActionResult> GetChats(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] string? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? groupId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25)
        {
            var filter = new ArchiveChatFilterDto
            {
                Search = search, Role = role, Status = status,
                FromDate = fromDate, ToDate = toDate,
                GroupId = groupId,
                Page = page, PageSize = pageSize
            };
            return Ok(await _service.GetChatConversationsAsync(filter));
        }

        [HttpGet("chats/{conversationId}")]
        public async Task<IActionResult> GetDetail(int conversationId)
        {
            var d = await _service.GetChatConversationDetailAsync(conversationId);
            return d == null ? NotFound() : Ok(d);
        }

        [HttpPost("chats/{conversationId}/archive")]
        public async Task<IActionResult> Archive(int conversationId)
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _service.ArchiveConversationAsync(conversationId, uid!);
            return Ok();
        }

        [HttpPost("chats/{conversationId}/restore")]
        public async Task<IActionResult> Restore(int conversationId)
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _service.RestoreConversationAsync(conversationId, uid!);
            return Ok();
        }
    }
}
