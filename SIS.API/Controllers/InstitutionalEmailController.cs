using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/institutional-email")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
    public class InstitutionalEmailController : ControllerBase
    {
        private readonly IInstitutionalEmailService _service;

        public InstitutionalEmailController(IInstitutionalEmailService service)
        {
            _service = service;
        }

        /// <summary>
        /// Ad + soyad əsasında unique @sis.az email təklif edir.
        /// GET /api/institutional-email/generate?firstName=Farid&amp;lastName=Mammadov
        /// → { "email": "faridm1@sis.az" }
        /// </summary>
        [HttpGet("generate")]
        public async Task<IActionResult> Generate([FromQuery] string firstName, [FromQuery] string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return BadRequest(new { message = "firstName və lastName tələb olunur" });

            var email = await _service.GenerateEmailAsync(firstName, lastName);
            return Ok(new { email });
        }
    }
}
