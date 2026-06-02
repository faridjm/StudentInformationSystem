using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Chat;
using SIS.BLL.Services.Interfaces;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ICurrentUser _currentUser;

        public ChatController(IChatService chatService, ICurrentUser currentUser)
        {
            _chatService = chatService;
            _currentUser = currentUser;
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = _currentUser.UserId!;
            var conversations = await _chatService.GetMyConversationsAsync(userId);
            return Ok(conversations);
        }

        [HttpPost("conversations")]
        public async Task<IActionResult> StartConversation([FromBody] StartConversationDto dto)
        {
            var userId = _currentUser.UserId!;
            var conv = await _chatService.GetOrCreateConversationAsync(userId, dto.GroupId, dto.TargetUserId);
            return Ok(conv);
        }

        [HttpGet("conversations/{id}/messages")]
        public async Task<IActionResult> GetMessages(int id, int take = 50, int skip = 0)
        {
            var userId = _currentUser.UserId!;
            if (!await _chatService.HasAccessAsync(id, userId))
                return Forbid();
            var messages = await _chatService.GetMessagesAsync(id, userId, take, skip);
            return Ok(messages);
        }

        [HttpPost("conversations/{id}/messages")]
        public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageDto dto)
        {
            var userId = _currentUser.UserId!;
            var msg = await _chatService.SendMessageAsync(userId, id, dto.Content);
            return Ok(msg);
        }

        [HttpPut("conversations/{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = _currentUser.UserId!;
            await _chatService.MarkAsReadAsync(id, userId);
            return Ok();
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _currentUser.UserId!;
            var count = await _chatService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }
    }
}
