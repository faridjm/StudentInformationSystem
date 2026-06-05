using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class OnlineLessonsController : Controller
    {
        private readonly IOnlineLessonApi _api;
        public OnlineLessonsController(IOnlineLessonApi api) { _api = api; }

        public async Task<IActionResult> Index()
        {
            var lessons = (await _api.GetAllAsync()).ToList();
            return View(lessons);
        }
    }
}
