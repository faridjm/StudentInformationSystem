using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIS.UI.Areas.Admin.Controllers
{
    /// <summary>
    /// Bu controller əvvəllər /Admin/PublicApplications altında işləyirdi.
    /// İndi public website idarəetməsi /PublicAdmin altına köçürülüb.
    /// Köhnə link-lər və bookmark-lar üçün burada yalnız redirect saxlayırıq.
    /// </summary>
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class PublicApplicationsController : Controller
    {
        public IActionResult Index() =>
            RedirectToAction("Index", "Applications", new { area = "PublicAdmin" });

        public IActionResult Details(int id) =>
            RedirectToAction("Details", "Applications", new { area = "PublicAdmin", id });

        [HttpPost, IgnoreAntiforgeryToken]
        public IActionResult UpdateStatus() =>
            RedirectToAction("Index", "Applications", new { area = "PublicAdmin" });

        [HttpPost, IgnoreAntiforgeryToken]
        public IActionResult UpdateNote() =>
            RedirectToAction("Index", "Applications", new { area = "PublicAdmin" });

        [HttpPost, IgnoreAntiforgeryToken]
        public IActionResult Convert() =>
            RedirectToAction("Index", "Applications", new { area = "PublicAdmin" });
    }
}
