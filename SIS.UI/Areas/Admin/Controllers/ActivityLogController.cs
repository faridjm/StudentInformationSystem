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

        public IActionResult Index()
        {
            return View(Enumerable.Empty<SIS.Entities.Models.ActivityLog>());
        }
    }
}
