using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Groups;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using System.Security.Claims;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _service;
        private readonly ITeacherService _teacherService;
        private readonly ICurriculumScheduleCalculatorService _scheduleCalculator;
        public GroupController(IGroupService service, ITeacherService teacherService,
            ICurriculumScheduleCalculatorService scheduleCalculator)
        {
            _service = service;
            _teacherService = teacherService;
            _scheduleCalculator = scheduleCalculator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var g = await _service.GetByIdAsync(id);
            return g == null ? NotFound() : Ok(g);
        }

        [HttpGet("my-groups")]
        [Authorize(Roles = RoleConstants.Teacher)]
        public async Task<IActionResult> GetMyGroups()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var teacher = await _teacherService.GetTeacherByUserIdAsync(userId!);
            if (teacher == null) return NotFound();
            return Ok(await _service.GetByTeacherAsync(teacher.Id));
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] CreateGroupDto dto)
            => Ok(await _service.CreateAsync(dto));

        /// <summary>
        /// Calculated EndDate preview: StartDate, dərs günləri və curriculum şablonuna (dərs sayı) görə
        /// qrupun bitmə tarixini hesablayır. Qrup yaradılmadan, yalnız önizləmə üçün.
        /// </summary>
        [HttpPost("preview-enddate")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> PreviewEndDate([FromBody] CurriculumScheduleRequest request)
            => Ok(await _scheduleCalculator.CalculateAsync(request));

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Update([FromBody] UpdateGroupDto dto)
        {
            await _service.UpdateAsync(dto);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }

        [HttpPost("{id}/generate-sessions")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GenerateSessions(int id)
        {
            await _service.GenerateSessionsAsync(id);
            return Ok(new { Message = "Dərs sessiyaları yaradıldı" });
        }
    }
}
