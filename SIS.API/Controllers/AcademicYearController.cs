using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.AcademicYear;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AcademicYearController : ControllerBase
    {
        private readonly IAcademicYearService _yearService;

        public AcademicYearController(IAcademicYearService yearService)
        {
            _yearService = yearService;
        }

        [HttpGet]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetAll()
        {
            var years = await _yearService.GetAllYearsAsync();
            return Ok(years);
        }

        [HttpGet("active")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetActive()
        {
            var year = await _yearService.GetActiveYearAsync();
            if (year == null) return NotFound();
            return Ok(year);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetById(int id)
        {
            var year = await _yearService.GetYearByIdAsync(id);
            if (year == null) return NotFound();
            return Ok(year);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] CreateAcademicYearDto dto)
        {
            await _yearService.CreateYearAsync(dto);
            return Ok(new { Message = "Dərs sezonu uğurla yaradıldı" });
        }

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Update([FromBody] UpdateAcademicYearDto dto)
        {
            await _yearService.UpdateYearAsync(dto);
            return Ok(new { Message = "Dərs sezonu uğurla yeniləndi" });
        }

        [HttpPut("{id}/set-active")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> SetActive(int id)
        {
            await _yearService.SetActiveYearAsync(id);
            return Ok(new { Message = "Dərs sezonu aktiv edildi" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _yearService.DeleteYearAsync(id);
            return Ok(new { Message = "Dərs sezonu uğurla silindi" });
        }
    }
}
