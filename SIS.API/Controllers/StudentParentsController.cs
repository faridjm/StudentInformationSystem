using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Parent;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    /// <summary>
    /// Tələbənin valideyn hesablarının idarəsi (yalnız Admin / SuperAdmin).
    /// Valideyn paneli read-only-dir; bu controller yalnız əlaqə idarəetməsidir.
    /// </summary>
    [ApiController]
    [Route("api/students/{studentId}/parents")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
    public class StudentParentsController : ControllerBase
    {
        private readonly IParentService _parentService;

        public StudentParentsController(IParentService parentService)
        {
            _parentService = parentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetParents(int studentId)
            => Ok(await _parentService.GetParentsForStudentAsync(studentId));

        [HttpPost]
        public async Task<IActionResult> Create(int studentId, [FromBody] CreateParentForStudentDto dto)
        {
            try { return Ok(await _parentService.CreateParentForStudentAsync(studentId, dto)); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPost("link")]
        public async Task<IActionResult> Link(int studentId, [FromBody] LinkExistingParentDto dto)
        {
            try
            {
                await _parentService.LinkExistingParentAsync(studentId, dto);
                return Ok(new { message = "Valideyn tələbəyə bağlandı" });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPatch("{parentUserId}/disable")]
        public async Task<IActionResult> Disable(int studentId, string parentUserId)
        {
            try
            {
                await _parentService.SetRelationActiveAsync(studentId, parentUserId, false);
                return Ok(new { message = "Əlaqə deaktiv edildi" });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPatch("{parentUserId}/enable")]
        public async Task<IActionResult> Enable(int studentId, string parentUserId)
        {
            try
            {
                await _parentService.SetRelationActiveAsync(studentId, parentUserId, true);
                return Ok(new { message = "Əlaqə aktiv edildi" });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }
    }
}
