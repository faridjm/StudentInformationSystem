using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.UI.Services;

namespace SIS.UI.Controllers
{
    [AllowAnonymous]
    public class PublicController : Controller
    {
        private readonly IPublicWebsiteService _site;
        public PublicController(IPublicWebsiteService site) { _site = site; }

        public async Task<IActionResult> Index()
            => View(await _site.GetHomeAsync());

        public async Task<IActionResult> Courses(string? q, string? department, string? level)
            => View(await _site.GetCoursesAsync(q, department, level));

        public async Task<IActionResult> CourseDetail(int id)
        {
            var vm = await _site.GetCourseDetailAsync(id);
            if (vm == null) return RedirectToAction("Courses");
            return View(vm);
        }

        public async Task<IActionResult> Teachers()
            => View(await _site.GetTeachersAsync());

        public async Task<IActionResult> About()
            => View(await _site.GetAboutContentAsync());

        public IActionResult Graduates()
            => View(_site.GetGraduates());

        public IActionResult Contact() => View();

        // Phase 1: mesaj qəbulu (struktur müraciət saxlama CourseApplication mərhələsindədir).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string fullName, string email, string? phone, string? message)
        {
            TempData["PubSuccess"] = "Mesajınız üçün təşəkkür edirik! Komandamız tezliklə sizinlə əlaqə saxlayacaq.";
            return RedirectToAction("Contact");
        }
    }
}
