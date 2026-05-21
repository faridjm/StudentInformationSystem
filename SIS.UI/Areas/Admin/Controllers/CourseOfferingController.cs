using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.CourseOffering;
using SIS.Common.Pagination;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class CourseOfferingController : Controller
    {
        private readonly ICourseOfferingApi _offeringApi;
        private readonly ICourseApi _courseApi;
        private readonly ITeacherApi _teacherApi;
        private readonly IAcademicTermApi _termApi;

        public CourseOfferingController(
            ICourseOfferingApi offeringApi,
            ICourseApi courseApi,
            ITeacherApi teacherApi,
            IAcademicTermApi termApi)
        {
            _offeringApi = offeringApi;
            _courseApi = courseApi;
            _teacherApi = teacherApi;
            _termApi = termApi;
        }

        public async Task<IActionResult> Index()
        {
            var offerings = await _offeringApi.GetAllAsync();
            return View(offerings);
        }

        public async Task<IActionResult> Create()
        {
            var courses = await _courseApi.GetAllAsync(new PagedRequest { PageSize = 100 });
            var teachers = await _teacherApi.GetAllAsync(new PagedRequest { PageSize = 100 });
            var terms = await _termApi.GetAllAsync();
            ViewBag.Courses = courses.Items;
            ViewBag.Teachers = teachers.Items;
            ViewBag.Terms = terms;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCourseOfferingDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadViewBag();
                return View(dto);
            }
            try
            {
                await _offeringApi.CreateAsync(dto);
                TempData["Success"] = "Kurs təklifi uğurla yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadViewBag();
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var offering = await _offeringApi.GetByIdAsync(id);
            if (offering == null) return NotFound();
            await LoadViewBag();
            var dto = new UpdateCourseOfferingDto
            {
                Id = offering.Id,
                TeacherId = offering.TeacherId,
                Capacity = offering.Capacity,
                Room = offering.Room
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCourseOfferingDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadViewBag();
                return View(dto);
            }
            try
            {
                await _offeringApi.UpdateAsync(dto);
                TempData["Success"] = "Kurs təklifi uğurla yeniləndi";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadViewBag();
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _offeringApi.DeleteAsync(id);
                TempData["Success"] = "Kurs təklifi uğurla silindi";
            }
            catch { TempData["Error"] = "Silmə zamanı xəta baş verdi"; }
            return RedirectToAction("Index");
        }

        private async Task LoadViewBag()
        {
            var courses = await _courseApi.GetAllAsync(new PagedRequest { PageSize = 100 });
            var teachers = await _teacherApi.GetAllAsync(new PagedRequest { PageSize = 100 });
            var terms = await _termApi.GetAllAsync();
            ViewBag.Courses = courses.Items;
            ViewBag.Teachers = teachers.Items;
            ViewBag.Terms = terms;
        }
    }
}
