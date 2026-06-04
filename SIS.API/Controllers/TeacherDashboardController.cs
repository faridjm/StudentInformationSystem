using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{RoleConstants.Teacher},{RoleConstants.Admin},{RoleConstants.SuperAdmin}")]
    public class TeacherDashboardController : ControllerBase
    {
        private readonly ITeacherDashboardService _service;
        private readonly ICurrentUser _currentUser;

        public TeacherDashboardController(ITeacherDashboardService service, ICurrentUser currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
            => Ok(await _service.GetDashboardAsync(_currentUser.UserId ?? ""));
    }
}
