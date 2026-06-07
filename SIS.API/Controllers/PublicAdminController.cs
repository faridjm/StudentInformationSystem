using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.PublicAdmin;
using SIS.BLL.Services.Interfaces;
using SIS.Entities.Enums;

namespace SIS.API.Controllers
{
    /// <summary>
    /// PublicAdmin panel API-si — public sayt kontentinin idarəetməsi.
    /// Yalnız SuperAdmin və Admin rollarına icazə verilir.
    /// </summary>
    [ApiController]
    [Route("api/publicadmin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class PublicAdminController : ControllerBase
    {
        private readonly IPublicAdminService _svc;
        public PublicAdminController(IPublicAdminService svc) { _svc = svc; }

        // ---------- DASHBOARD ----------
        [HttpGet("stats")]
        public async Task<IActionResult> Stats() => Ok(await _svc.GetStatsAsync());

        // ---------- COURSE PROFILES ----------
        [HttpGet("course-profiles")]
        public async Task<IActionResult> ListCourseProfiles() => Ok(await _svc.GetCourseProfilesAsync());

        [HttpGet("course-profiles/{courseId:int}")]
        public async Task<IActionResult> GetCourseProfile(int courseId)
        {
            var p = await _svc.GetCourseProfileAsync(courseId);
            return p == null ? NotFound() : Ok(p);
        }

        [HttpPost("course-profiles")]
        public async Task<IActionResult> UpsertCourseProfile([FromBody] PublicCourseProfileEditDto dto)
        { await _svc.UpsertCourseProfileAsync(dto); return NoContent(); }

        [HttpPost("course-profiles/{courseId:int}/toggle-visibility")]
        public async Task<IActionResult> ToggleCourseVisibility(int courseId)
        {
            try { await _svc.ToggleCourseVisibilityAsync(courseId); return NoContent(); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // ---------- TEACHER PROFILES ----------
        [HttpGet("teacher-profiles")]
        public async Task<IActionResult> ListTeacherProfiles() => Ok(await _svc.GetTeacherProfilesAsync());

        [HttpGet("teacher-profiles/{teacherId:int}")]
        public async Task<IActionResult> GetTeacherProfile(int teacherId)
        {
            var p = await _svc.GetTeacherProfileAsync(teacherId);
            return p == null ? NotFound() : Ok(p);
        }

        [HttpPost("teacher-profiles")]
        public async Task<IActionResult> UpsertTeacherProfile([FromBody] PublicTeacherProfileEditDto dto)
        { await _svc.UpsertTeacherProfileAsync(dto); return NoContent(); }

        [HttpPost("teacher-profiles/{teacherId:int}/toggle-visibility")]
        public async Task<IActionResult> ToggleTeacherVisibility(int teacherId)
        {
            try { await _svc.ToggleTeacherVisibilityAsync(teacherId); return NoContent(); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // ---------- FACILITIES ----------
        [HttpGet("facilities")]
        public async Task<IActionResult> ListFacilities([FromQuery] bool onlyVisible = false)
            => Ok(await _svc.GetFacilitiesAsync(onlyVisible));
        [HttpGet("facilities/{id:int}")]
        public async Task<IActionResult> GetFacility(int id)
        { var d = await _svc.GetFacilityAsync(id); return d == null ? NotFound() : Ok(d); }
        [HttpPost("facilities")]
        public async Task<IActionResult> CreateFacility([FromBody] PublicFacilityDto dto)
            => Ok(new { id = await _svc.CreateFacilityAsync(dto) });
        [HttpPut("facilities")]
        public async Task<IActionResult> UpdateFacility([FromBody] PublicFacilityDto dto)
        { try { await _svc.UpdateFacilityAsync(dto); return NoContent(); } catch (KeyNotFoundException) { return NotFound(); } }
        [HttpDelete("facilities/{id:int}")]
        public async Task<IActionResult> DeleteFacility(int id)
        { try { await _svc.DeleteFacilityAsync(id); return NoContent(); } catch (KeyNotFoundException) { return NotFound(); } }

        // ---------- GRADUATES ----------
        [HttpGet("graduates")]
        public async Task<IActionResult> ListGraduates([FromQuery] bool onlyVisible = false)
            => Ok(await _svc.GetGraduatesAsync(onlyVisible));
        [HttpGet("graduates/{id:int}")]
        public async Task<IActionResult> GetGraduate(int id)
        { var d = await _svc.GetGraduateAsync(id); return d == null ? NotFound() : Ok(d); }
        [HttpPost("graduates")]
        public async Task<IActionResult> CreateGraduate([FromBody] PublicGraduateStoryDto dto)
            => Ok(new { id = await _svc.CreateGraduateAsync(dto) });
        [HttpPut("graduates")]
        public async Task<IActionResult> UpdateGraduate([FromBody] PublicGraduateStoryDto dto)
        { try { await _svc.UpdateGraduateAsync(dto); return NoContent(); } catch (KeyNotFoundException) { return NotFound(); } }
        [HttpDelete("graduates/{id:int}")]
        public async Task<IActionResult> DeleteGraduate(int id)
        { try { await _svc.DeleteGraduateAsync(id); return NoContent(); } catch (KeyNotFoundException) { return NotFound(); } }

        // ---------- FAQ ----------
        [HttpGet("faqs")]
        public async Task<IActionResult> ListFaqs([FromQuery] bool onlyVisible = false)
            => Ok(await _svc.GetFaqsAsync(onlyVisible));
        [HttpGet("faqs/{id:int}")]
        public async Task<IActionResult> GetFaq(int id)
        { var d = await _svc.GetFaqAsync(id); return d == null ? NotFound() : Ok(d); }
        [HttpPost("faqs")]
        public async Task<IActionResult> CreateFaq([FromBody] PublicFaqDto dto)
            => Ok(new { id = await _svc.CreateFaqAsync(dto) });
        [HttpPut("faqs")]
        public async Task<IActionResult> UpdateFaq([FromBody] PublicFaqDto dto)
        { try { await _svc.UpdateFaqAsync(dto); return NoContent(); } catch (KeyNotFoundException) { return NotFound(); } }
        [HttpDelete("faqs/{id:int}")]
        public async Task<IActionResult> DeleteFaq(int id)
        { try { await _svc.DeleteFaqAsync(id); return NoContent(); } catch (KeyNotFoundException) { return NotFound(); } }

        // ---------- CONTACT MESSAGES (admin views) ----------
        [HttpGet("contact-messages")]
        public async Task<IActionResult> ListContact([FromQuery] ContactMessageStatus? status = null)
            => Ok(await _svc.GetContactMessagesAsync(status));
        [HttpGet("contact-messages/{id:int}")]
        public async Task<IActionResult> GetContact(int id)
        { var d = await _svc.GetContactMessageAsync(id); return d == null ? NotFound() : Ok(d); }
        [HttpPut("contact-messages/{id:int}/status")]
        public async Task<IActionResult> SetContactStatus(int id, [FromBody] UpdateContactStatusDto dto)
        { try { await _svc.UpdateContactStatusAsync(id, dto); return NoContent(); } catch (KeyNotFoundException) { return NotFound(); } }
        [HttpPut("contact-messages/{id:int}/note")]
        public async Task<IActionResult> SetContactNote(int id, [FromBody] UpdateContactNoteDto dto)
        { try { await _svc.UpdateContactNoteAsync(id, dto); return NoContent(); } catch (KeyNotFoundException) { return NotFound(); } }
    }

    /// <summary>Public form (anonymous) Contact message submit endpoint-i.</summary>
    [ApiController]
    [Route("api/public/contact")]
    [AllowAnonymous]
    public class PublicContactController : ControllerBase
    {
        private readonly IPublicAdminService _svc;
        public PublicContactController(IPublicAdminService svc) { _svc = svc; }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] CreatePublicContactMessageDto dto)
        {
            try
            {
                var id = await _svc.CreateContactMessageAsync(dto);
                return Ok(new { id, message = "Mesajınız qəbul edildi. Komandamız tezliklə əlaqə saxlayacaq." });
            }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        }
    }
}
