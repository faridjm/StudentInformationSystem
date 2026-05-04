using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.CourseOffering;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CourseOfferingController : ControllerBase
    {
        private readonly ICourseOfferingService _offeringService;

        public CourseOfferingController(ICourseOfferingService offeringService)
        {
            _offeringService = offeringService;
        }

        [HttpGet]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetAll()
        {
            var offerings = await _offeringService.GetAllOfferingsAsync();
            return Ok(offerings);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetById(int id)
        {
            var offering = await _offeringService.GetOfferingByIdAsync(id);
            if (offering == null) return NotFound();
            return Ok(offering);
        }

        [HttpGet("active")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher},{RoleConstants.Student}")]
        public async Task<IActionResult> GetActive()
        {
            var offerings = await _offeringService.GetActiveOfferingsAsync();
            return Ok(offerings);
        }

        [HttpGet("teacher/{teacherId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}")]
        public async Task<IActionResult> GetByTeacher(int teacherId)
        {
            var offerings = await _offeringService.GetOfferingsByTeacherAsync(teacherId);
            return Ok(offerings);
        }

        [HttpGet("term/{termId}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Moderator},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> GetByTerm(int termId)
        {
            var offerings = await _offeringService.GetOfferingsByTermAsync(termId);
            return Ok(offerings);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] CreateCourseOfferingDto dto)
        {
            await _offeringService.CreateOfferingAsync(dto);
            return Ok(new { Message = "Kurs təklifi uğurla yaradıldı" });
        }

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Update([FromBody] UpdateCourseOfferingDto dto)
        {
            await _offeringService.UpdateOfferingAsync(dto);
            return Ok(new { Message = "Kurs təklifi uğurla yeniləndi" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _offeringService.DeleteOfferingAsync(id);
            return Ok(new { Message = "Kurs təklifi uğurla silindi" });
        }
    }
}
