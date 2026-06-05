using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Announcement;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;

        public AnnouncementController(
            IAnnouncementService announcementService,
            IStudentService studentService,
            ITeacherService teacherService)
        {
            _announcementService = announcementService;
            _studentService = studentService;
            _teacherService = teacherService;
        }

        /// <summary>Admin görüşü — bütün elanlar (recipient/read/email statları ilə).</summary>
        [HttpGet]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetAll()
            => Ok(await _announcementService.GetAllAsync());

        [HttpGet("global")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetGlobal()
            => Ok(await _announcementService.GetAllAsync());

        [HttpGet("group/{groupId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> GetByGroup(int groupId)
            => Ok(await _announcementService.GetByGroupAsync(groupId));

        [HttpGet("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var d = await _announcementService.GetDetailAsync(id);
            return d == null ? NotFound() : Ok(d);
        }

        /// <summary>Cari istifadəçi (Student/Teacher) öz aldığı elanlar.</summary>
        [HttpGet("my")]
        [Authorize(Roles = $"{RoleConstants.Student},{RoleConstants.Teacher}")]
        public async Task<IActionResult> GetMy()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Ok(await _announcementService.GetForUserAsync(userId!));
        }

        // Geriyə uyğun metodlar (köhnə UI çağırışları üçün) — recipient əsasında işləyir.
        [HttpGet("my-announcements")]
        [Authorize(Roles = RoleConstants.Student)]
        public async Task<IActionResult> GetMyAnnouncements()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Ok(await _announcementService.GetForUserAsync(userId!));
        }

        [HttpGet("my-teacher-announcements")]
        [Authorize(Roles = RoleConstants.Teacher)]
        public async Task<IActionResult> GetMyTeacherAnnouncements()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Ok(await _announcementService.GetForUserAsync(userId!));
        }

        [HttpPost("{id}/read")]
        [Authorize(Roles = $"{RoleConstants.Student},{RoleConstants.Teacher}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _announcementService.MarkAsReadAsync(id, userId!);
            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] CreateAnnouncementDto dto)
        {
            var authorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _announcementService.CreateAnnouncementAsync(dto, authorId!);
            return Ok(new { Message = "Elan uğurla yaradıldı" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _announcementService.DeleteAnnouncementAsync(id);
            return Ok(new { Message = "Elan uğurla silindi" });
        }

        /// <summary>SelectedUsers üçün admin axtarışı.</summary>
        [HttpGet("search-users")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
            => Ok(await _announcementService.SearchUsersAsync(query));
    }
}
