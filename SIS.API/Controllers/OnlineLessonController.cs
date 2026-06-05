using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.OnlineLesson;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using System.Security.Claims;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OnlineLessonController : ControllerBase
    {
        private readonly IOnlineLessonService _service;
        private readonly ITeacherService _teacherService;
        private readonly IStudentService _studentService;

        public OnlineLessonController(IOnlineLessonService service,
            ITeacherService teacherService, IStudentService studentService)
        {
            _service = service;
            _teacherService = teacherService;
            _studentService = studentService;
        }

        private string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private async Task<int> ResolveTeacherIdAsync()
        {
            var teacher = await _teacherService.GetTeacherByUserIdAsync(UserId!)
                ?? throw new UnauthorizedAccessException("Müəllim tapılmadı");
            return teacher.Id;
        }

        private async Task<int> ResolveStudentIdAsync()
        {
            var student = await _studentService.GetStudentByUserIdAsync(UserId!)
                ?? throw new UnauthorizedAccessException("Tələbə tapılmadı");
            return student.Id;
        }

        // ── Müəllim: lifecycle ──

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Teacher},{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] CreateOnlineLessonDto dto)
        {
            var teacherId = await ResolveTeacherIdAsync();
            var id = await _service.CreateOnlineLessonAsync(dto, teacherId);
            return Ok(new { id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{RoleConstants.Teacher},{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOnlineLessonDto dto)
        {
            dto.Id = id;
            var teacherId = await ResolveTeacherIdAsync();
            await _service.UpdateOnlineLessonAsync(dto, teacherId);
            return Ok();
        }

        [HttpPost("session/{sessionId}/start")]
        [Authorize(Roles = $"{RoleConstants.Teacher},{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Start(int sessionId)
        {
            var teacherId = await ResolveTeacherIdAsync();
            var result = await _service.StartOnlineLessonAsync(sessionId, teacherId);
            return Ok(result);
        }

        [HttpPost("{id}/end")]
        [Authorize(Roles = $"{RoleConstants.Teacher},{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> End(int id)
        {
            var teacherId = await ResolveTeacherIdAsync();
            await _service.EndOnlineLessonAsync(id, teacherId);
            return Ok();
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = $"{RoleConstants.Teacher},{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var teacherId = await ResolveTeacherIdAsync();
            await _service.CancelOnlineLessonAsync(id, teacherId);
            return Ok();
        }

        // ── Tələbə: qoşulma ──

        [HttpPost("{id}/join")]
        [Authorize(Roles = RoleConstants.Student)]
        public async Task<IActionResult> Join(int id)
        {
            var studentId = await ResolveStudentIdAsync();
            var result = await _service.JoinOnlineLessonAsync(id, studentId);
            return Ok(result);
        }

        // ── Sorğular ──

        [HttpGet("group/{groupId}")]
        [Authorize(Roles = $"{RoleConstants.Teacher},{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetByGroup(int groupId)
        {
            var teacherId = await ResolveTeacherIdAsync();
            return Ok(await _service.GetTeacherGroupOnlineLessonsAsync(groupId, teacherId));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = UserId ?? throw new UnauthorizedAccessException();
            var dto = await _service.GetOnlineLessonAsync(id, userId);
            return dto == null ? Forbid() : Ok(dto);
        }

        [HttpGet("student")]
        [Authorize(Roles = RoleConstants.Student)]
        public async Task<IActionResult> GetForStudent()
        {
            var studentId = await ResolveStudentIdAsync();
            return Ok(await _service.GetStudentOnlineLessonsAsync(studentId));
        }

        [HttpGet("{id}/participants")]
        [Authorize(Roles = $"{RoleConstants.Teacher},{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetParticipants(int id)
            => Ok(await _service.GetOnlineLessonParticipantsAsync(id));

        [HttpPost("{id}/mark-present")]
        [Authorize(Roles = $"{RoleConstants.Teacher},{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> MarkPresent(int id)
        {
            var teacherId = await ResolveTeacherIdAsync();
            var result = await _service.MarkJoinedStudentsAsPresentAsync(id, teacherId);
            return Ok(result);
        }

        // ── Daxili Live Classroom Chat ──

        [HttpGet("{id}/chat")]
        public async Task<IActionResult> GetChat(int id)
        {
            var userId = UserId ?? throw new UnauthorizedAccessException();
            return Ok(await _service.GetClassroomChatHistoryAsync(id, userId));
        }

        [HttpPost("{id}/chat")]
        public async Task<IActionResult> SendChat(int id, [FromBody] SendClassroomChatDto dto)
        {
            var userId = UserId ?? throw new UnauthorizedAccessException();
            var result = await _service.SaveClassroomChatMessageAsync(id, userId, dto.Text ?? "");
            return Ok(result);
        }

        // ── Admin icmalı ──

        [HttpGet("all")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllOnlineLessonsAsync());
    }
}
