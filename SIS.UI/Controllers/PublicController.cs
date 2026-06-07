using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Public;
using SIS.BLL.Services.DTOs.PublicAdmin;
using SIS.UI.ApiClients;
using SIS.UI.Services;

namespace SIS.UI.Controllers
{
    [AllowAnonymous]
    public class PublicController : Controller
    {
        private readonly IPublicWebsiteService _site;
        private readonly ICourseApplicationApi _applicationApi;
        private readonly IPublicContactApi _contactApi;

        public PublicController(IPublicWebsiteService site,
            ICourseApplicationApi applicationApi,
            IPublicContactApi contactApi)
        {
            _site = site;
            _applicationApi = applicationApi;
            _contactApi = contactApi;
        }

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

        // ===== Contact form (PublicContactMessage database-ə yazılır) =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string fullName, string email, string? phone, string? subject, string? message)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                TempData["PubError"] = "Ad, email və mesaj mütləqdir.";
                return RedirectToAction("Contact");
            }
            try
            {
                var resp = await _contactApi.SubmitAsync(new CreatePublicContactMessageDto
                {
                    FullName = fullName.Trim(),
                    Email = email.Trim(),
                    Phone = phone,
                    Subject = subject,
                    Message = message
                });
                TempData["PubSuccess"] = resp.IsSuccessStatusCode
                    ? "Mesajınız üçün təşəkkür edirik! Komandamız tezliklə sizinlə əlaqə saxlayacaq."
                    : "Mesaj göndərilə bilmədi. Bir az sonra yenidən cəhd edin.";
            }
            catch
            {
                TempData["PubError"] = "Texniki səbəbdən mesaj göndərilmədi. Zəhmət olmasa yenidən cəhd edin.";
            }
            return RedirectToAction("Contact");
        }

        // ===== Public Apply form (CourseDetail modal-ından) =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int? courseId, int? groupId,
            string fullName, string email, string phone, string? message)
        {
            // Server-side basic validation (client-side mövcuddur)
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email)    ||
                string.IsNullOrWhiteSpace(phone))
            {
                TempData["ApplyError"] = "Ad, email və telefon mütləqdir.";
                return RedirectToCourseOrCourses(courseId);
            }

            try
            {
                var resp = await _applicationApi.CreateAsync(new CreateCourseApplicationDto
                {
                    FullName = fullName.Trim(),
                    Email = email.Trim(),
                    Phone = phone.Trim(),
                    CourseId = courseId,
                    GroupId = groupId,
                    Message = message
                });

                if (resp.IsSuccessStatusCode)
                {
                    TempData["ApplySuccess"] =
                        "Müraciətiniz qəbul edildi. Komandamız sizinlə əlaqə saxlayacaq.";
                }
                else
                {
                    TempData["ApplyError"] =
                        "Müraciət göndərilə bilmədi. Bir az sonra yenidən cəhd edin.";
                }
            }
            catch
            {
                TempData["ApplyError"] =
                    "Texniki səbəbdən müraciət göndərilmədi. Zəhmət olmasa yenidən cəhd edin.";
            }

            return RedirectToCourseOrCourses(courseId);
        }

        private IActionResult RedirectToCourseOrCourses(int? courseId)
            => courseId.HasValue
                ? RedirectToAction("CourseDetail", new { id = courseId.Value })
                : RedirectToAction("Courses");
    }
}
