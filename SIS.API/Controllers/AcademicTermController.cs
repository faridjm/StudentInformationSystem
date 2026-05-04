using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.AcademicTerm;
using SIS.BLL.Services.DTOs.Auth;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AcademicTermController : ControllerBase
    {
        private readonly IAcademicTermService _termService;

        public AcademicTermController(IAcademicTermService termService)
        {
            _termService = termService;
        }

        [HttpGet]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetAll()
        {
            var terms = await _termService.GetAllTermsAsync();
            return Ok(terms);
        }

        [HttpGet("active")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetActive()
        {
            var term = await _termService.GetActiveTermAsync();
            if (term == null) return NotFound();
            return Ok(term);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetById(int id)
        {
            var term = await _termService.GetTermByIdAsync(id);
            if (term == null) return NotFound();
            return Ok(term);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] CreateAcademicTermDto dto)
        {
            await _termService.CreateTermAsync(dto);
            return Ok(new { Message = "Termin uğurla yaradıldı" });
        }

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Update([FromBody] UpdateAcademicTermDto dto)
        {
            await _termService.UpdateTermAsync(dto);
            return Ok(new { Message = "Termin uğurla yeniləndi" });
        }

        [HttpPut("{id}/set-active")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> SetActive(int id)
        {
            await _termService.SetActiveTermAsync(id);
            return Ok(new { Message = "Termin aktiv edildi" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _termService.DeleteTermAsync(id);
            return Ok(new { Message = "Termin uğurla silindi" });
        }
    }
}
