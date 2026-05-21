using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Course;
using SIS.Common.Pagination;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class CourseController : Controller
    {
        private readonly ICourseApi _courseApi;
        private readonly IDepartmentApi _departmentApi;

        public CourseController(ICourseApi courseApi, IDepartmentApi departmentApi)
        {
            _courseApi = courseApi;
            _departmentApi = departmentApi;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var request = new PagedRequest { Page = page, PageSize = 10, SearchTerm = search };
            var result = await _courseApi.GetAllAsync(request);
            ViewBag.SearchTerm = search;
            return View(result);
        }

        public async Task<IActionResult> Create()
        {
            var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
            ViewBag.Departments = departments.Items;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCourseDto dto)
        {
            if (!ModelState.IsValid)
            {
                var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
                ViewBag.Departments = departments.Items;
                return View(dto);
            }
            try
            {
                await _courseApi.CreateAsync(dto);
                TempData["Success"] = "Kurs uğurla yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
                ViewBag.Departments = departments.Items;
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseApi.GetByIdAsync(id);
            if (course == null) return NotFound();

            var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
            ViewBag.Departments = departments.Items;

            var dto = new UpdateCourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Credits = course.Credits,
                Description = course.Description,
                DepartmentId = course.DepartmentId
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCourseDto dto)
        {
            if (!ModelState.IsValid)
            {
                var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
                ViewBag.Departments = departments.Items;
                return View(dto);
            }
            try
            {
                await _courseApi.UpdateAsync(dto);
                TempData["Success"] = "Kurs uğurla yeniləndi";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
                ViewBag.Departments = departments.Items;
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _courseApi.DeleteAsync(id);
                TempData["Success"] = "Kurs uğurla silindi";
            }
            catch { TempData["Error"] = "Silmə zamanı xəta baş verdi"; }
            return RedirectToAction("Index");
        }
    }
}
