using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.PublicAdmin;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.PublicAdmin.Controllers
{
    [Area("PublicAdmin")]
    [Authorize(Policy = "PublicAdminOnly")]
    public class DashboardController : Controller
    {
        private readonly IPublicAdminApi _api;
        public DashboardController(IPublicAdminApi api) { _api = api; }

        public async Task<IActionResult> Index()
        {
            PublicAdminStatsDto stats;
            try { stats = await _api.StatsAsync(); }
            catch { stats = new PublicAdminStatsDto(); }
            return View(stats);
        }
    }
}
