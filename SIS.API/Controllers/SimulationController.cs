using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.API.Services;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulationController : ControllerBase
    {
        private readonly SimulationService _sim;

        public SimulationController(SimulationService sim)
        {
            _sim = sim;
        }

        [HttpPost("set")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public IActionResult Set([FromBody] SetSimulationDto dto)
        {
            _sim.Set(dto.Date);
            return Ok(new { Message = "Simulasiya aktiv edildi", Date = dto.Date.ToString("yyyy-MM-dd") });
        }

        [HttpPost("clear")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
        public IActionResult Clear()
        {
            _sim.Clear();
            return Ok(new { Message = "Simulasiya söndürüldü" });
        }

        [HttpGet("status")]
        [Authorize]
        public IActionResult Status()
        {
            return Ok(new
            {
                IsActive = _sim.IsActive,
                Date = _sim.OverrideDate?.ToString("yyyy-MM-dd")
            });
        }
    }

    public class SetSimulationDto
    {
        public DateTime Date { get; set; }
    }
}
