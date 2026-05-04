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

        public AnnouncementController(
            IAnnouncementService announcementService,
            IStudentService studentService)
        {
            _announcementService = announcementService;
            _studentService = studentService;
        }

        [HttpGet("global")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetGlobal()
        {
            var announcements = await _announcementService.GetGlobalAnnouncementsAsync();
            return Ok(announcements);
        }

        [HttpGet("course/{courseOfferingId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetByCourse(int courseOfferingId)
        {
            var announcements = await _announcementService.GetCourseAnnouncementsAsync(courseOfferingId);
            return Ok(announcements);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetByStudent(int studentId)
        {
            var announcements = await _announcementService.GetAnnouncementsForStudentAsync(studentId);
            return Ok(announcements);
        }

        [HttpGet("my-announcements")]
        [Authorize(Roles = RoleConstants.Student)]
        public async Task<IActionResult> GetMyAnnouncements()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var student = await _studentService.GetStudentByUserIdAsync(userId!);
            if (student == null) return NotFound();
            var announcements = await _announcementService.GetAnnouncementsForStudentAsync(student.Id);
            return Ok(announcements);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
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
    }
}
