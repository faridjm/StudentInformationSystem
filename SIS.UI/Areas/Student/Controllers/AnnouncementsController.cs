using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Policy = "StudentOnly")]
    public class AnnouncementsController : Controller
    {
        private readonly IAnnouncementApi _announcementApi;

        public AnnouncementsController(IAnnouncementApi announcementApi)
        {
            _announcementApi = announcementApi;
        }

        public async Task<IActionResult> Index()
        {
            var announcements = await _announcementApi.GetMyAnnouncementsAsync();
            return View(announcements);
        }
    }
}
