using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Enrollment;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentApi _enrollmentApi;
        private readonly IStudentApi _studentApi;
        private readonly ICourseOfferingApi _offeringApi;

        public EnrollmentController(
            IEnrollmentApi enrollmentApi,
            IStudentApi studentApi,
            ICourseOfferingApi offeringApi)
        {
            _enrollmentApi = enrollmentApi;
            _studentApi = studentApi;
            _offeringApi = offeringApi;
        }

        public async Task<IActionResult> Index()
        {
            var enrollments = await _enrollmentApi.GetAllAsync();
            return View(enrollments);
        }

        public async Task<IActionResult> Create()
        {
            var students = await _studentApi.GetAllAsync(new Common.Pagination.PagedRequest { PageSize = 100 });
            var offerings = await _offeringApi.GetActiveAsync();
            ViewBag.Students = students.Items;
            ViewBag.Offerings = offerings;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEnrollmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var students = await _studentApi.GetAllAsync(new Common.Pagination.PagedRequest { PageSize = 100 });
                var offerings = await _offeringApi.GetActiveAsync();
                ViewBag.Students = students.Items;
                ViewBag.Offerings = offerings;
                return View(dto);
            }
            try
            {
                await _enrollmentApi.EnrollAsync(dto);
                TempData["Success"] = "Qeydiyyat uğurla tamamlandı";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var students = await _studentApi.GetAllAsync(new Common.Pagination.PagedRequest { PageSize = 100 });
                var offerings = await _offeringApi.GetActiveAsync();
                ViewBag.Students = students.Items;
                ViewBag.Offerings = offerings;
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Drop(int id)
        {
            try
            {
                await _enrollmentApi.DropAsync(id);
                TempData["Success"] = "Qeydiyyat ləğv edildi";
            }
            catch { TempData["Error"] = "Xəta baş verdi"; }
            return RedirectToAction("Index");
        }
    }
}
