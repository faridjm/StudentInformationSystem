using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class ActivityLogController : Controller
    {
        private readonly IActivityLogApi _activityLogApi;

        public ActivityLogController(IActivityLogApi activityLogApi)
        {
            _activityLogApi = activityLogApi;
        }

        public async Task<IActionResult> Index(int count = 50)
        {
            var logs = await _activityLogApi.GetRecentAsync(count);
            ViewBag.Count = count;
            return View(logs);
        }
    }
}
