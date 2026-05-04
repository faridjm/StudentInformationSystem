using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.AcademicProgram;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AcademicProgramController : ControllerBase
    {
        private readonly IAcademicProgramService _programService;

        public AcademicProgramController(IAcademicProgramService programService)
        {
            _programService = programService;
        }

        [HttpGet]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetAll()
        {
            var programs = await _programService.GetAllProgramsAsync();
            return Ok(programs);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetById(int id)
        {
            var program = await _programService.GetProgramByIdAsync(id);
            if (program == null) return NotFound();
            return Ok(program);
        }

        [HttpGet("department/{departmentId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetByDepartment(int departmentId)
        {
            var programs = await _programService.GetProgramsByDepartmentAsync(departmentId);
            return Ok(programs);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] CreateAcademicProgramDto dto)
        {
            await _programService.CreateProgramAsync(dto);
            return Ok(new { Message = "Proqram uğurla yaradıldı" });
        }

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Update([FromBody] UpdateAcademicProgramDto dto)
        {
            await _programService.UpdateProgramAsync(dto);
            return Ok(new { Message = "Proqram uğurla yeniləndi" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _programService.DeleteProgramAsync(id);
            return Ok(new { Message = "Proqram uğurla silindi" });
        }
    }
}
