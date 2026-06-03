using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.Interfaces;
using SIS.Entities.Enums;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(UserId!);
            return Ok(notifications);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 10)
            => Ok(await _notificationService.GetRecentAsync(UserId!, count));

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] bool? isRead, [FromQuery] NotificationType? type,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
            => Ok(await _notificationService.GetPagedAsync(UserId!, isRead, type, page, pageSize));

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var ok = await _notificationService.MarkAsReadAsync(id, UserId!);
            return ok ? Ok() : NotFound();
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var count = await _notificationService.GetUnreadCountAsync(userId!);
            return Ok(new { Count = count });
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _notificationService.MarkAllAsReadAsync(userId!);
            return Ok(new { Message = "Bütün bildirişlər oxundu" });
        }
    }
}
