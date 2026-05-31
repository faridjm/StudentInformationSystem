using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Course;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class CourseController : Controller
    {
        private readonly ICourseApi _courseApi;
        public CourseController(ICourseApi courseApi) { _courseApi = courseApi; }

        public async Task<IActionResult> Index()
            => View(await _courseApi.GetAllAsync());

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateCourseDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _courseApi.CreateAsync(dto);
            TempData["Success"] = "Kurs yaradıldı";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseApi.GetByIdAsync(id);
            return View(new UpdateCourseDto { Id = course.Id, Name = course.Name, Description = course.Description, IsActive = course.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCourseDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _courseApi.UpdateAsync(dto);
            TempData["Success"] = "Dəyişikliklər saxlandı";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _courseApi.DeleteAsync(id);
            TempData["Success"] = "Kurs silindi";
            return RedirectToAction("Index");
        }
    }
}
