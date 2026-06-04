using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Dashboard;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class DashboardController : Controller
    {
        private readonly ITeacherDashboardApi _dashboardApi;

        public DashboardController(ITeacherDashboardApi dashboardApi)
        {
            _dashboardApi = dashboardApi;
        }

        public async Task<IActionResult> Index()
        {
            TeacherDashboardDto model;
            try { model = await _dashboardApi.GetAsync(); }
            catch { model = new TeacherDashboardDto(); }
            return View(model);
        }
    }
}
