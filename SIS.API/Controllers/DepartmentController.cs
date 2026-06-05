using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Department;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _service;
        private readonly IAuditLogService _audit;

        public DepartmentController(IDepartmentService service, IAuditLogService audit)
        {
            _service = service;
            _audit = audit;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var d = await _service.GetByIdAsync(id);
            return d == null ? NotFound() : Ok(d);
        }

        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
        {
            var result = await _service.CreateAsync(dto);
            await _audit.LogAsync("CreateDepartment", "Department", result.Id.ToString(),
                $"Şöbə yaradıldı: {result.Name}");
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Update([FromBody] UpdateDepartmentDto dto)
        {
            await _service.UpdateAsync(dto);
            await _audit.LogAsync("UpdateDepartment", "Department", dto.Id.ToString(),
                $"Şöbə yeniləndi: {dto.Name}");
            return Ok();
        }

        [HttpPost("{id}/active")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> SetActive(int id, [FromQuery] bool isActive)
        {
            await _service.SetActiveAsync(id, isActive);
            await _audit.LogAsync(isActive ? "ActivateDepartment" : "DeactivateDepartment",
                "Department", id.ToString(),
                isActive ? "Şöbə aktiv edildi" : "Şöbə deaktiv edildi");
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            await _audit.LogAsync("DeleteDepartment", "Department", id.ToString(), "Şöbə silindi");
            return Ok();
        }
    }
}
