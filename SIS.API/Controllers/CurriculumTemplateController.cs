using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Curriculum;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
    public class CurriculumTemplateController : ControllerBase
    {
        private readonly ICurriculumTemplateService _service;
        public CurriculumTemplateController(ICurriculumTemplateService service) { _service = service; }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId) => Ok(await _service.GetByCourseAsync(courseId));

        [HttpGet("course/{courseId}/active")]
        public async Task<IActionResult> GetActive(int courseId)
        {
            var t = await _service.GetActiveByCourseAsync(courseId);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var t = await _service.GetDetailAsync(id);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCurriculumTemplateDto dto)
            => Ok(await _service.CreateAsync(dto));

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCurriculumTemplateDto dto)
        {
            await _service.UpdateAsync(dto);
            return Ok();
        }

        [HttpPost("{id}/active")]
        public async Task<IActionResult> SetActive(int id, [FromQuery] bool active)
        {
            await _service.SetActiveAsync(id, active);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] CreateTemplateItemDto dto)
            => Ok(await _service.AddItemAsync(dto));

        [HttpPut("items")]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateTemplateItemDto dto)
            => Ok(await _service.UpdateItemAsync(dto));

        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            await _service.DeleteItemAsync(itemId);
            return Ok();
        }

        /// <summary>Şablonda növbəti boş SessionNumber.</summary>
        [HttpGet("{templateId}/next-session-number")]
        public async Task<IActionResult> GetNextSessionNumber(int templateId)
            => Ok(new { nextSessionNumber = await _service.GetNextSessionNumberAsync(templateId) });
    }
}
