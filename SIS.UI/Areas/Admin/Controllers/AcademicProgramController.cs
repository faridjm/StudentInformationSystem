using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.AcademicProgram;
using SIS.Common.Pagination;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AcademicProgramController : Controller
    {
        private readonly IAcademicProgramApi _programApi;
        private readonly IDepartmentApi _departmentApi;

        public AcademicProgramController(IAcademicProgramApi programApi, IDepartmentApi departmentApi)
        {
            _programApi = programApi;
            _departmentApi = departmentApi;
        }

        public async Task<IActionResult> Index()
        {
            var programs = await _programApi.GetAllAsync();
            return View(programs);
        }

        public async Task<IActionResult> Create()
        {
            var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
            ViewBag.Departments = departments.Items;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAcademicProgramDto dto)
        {
            if (!ModelState.IsValid)
            {
                var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
                ViewBag.Departments = departments.Items;
                return View(dto);
            }
            try
            {
                await _programApi.CreateAsync(dto);
                TempData["Success"] = "Proqram uğurla yaradıldı";
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
            var program = await _programApi.GetByIdAsync(id);
            if (program == null) return NotFound();

            var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
            ViewBag.Departments = departments.Items;

            var dto = new UpdateAcademicProgramDto
            {
                Id = program.Id,
                Name = program.Name,
                Code = program.Code,
                DepartmentId = program.DepartmentId,
                DurationYears = program.DurationYears,
                Description = program.Description
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateAcademicProgramDto dto)
        {
            if (!ModelState.IsValid)
            {
                var departments = await _departmentApi.GetAllAsync(new PagedRequest { PageSize = 100 });
                ViewBag.Departments = departments.Items;
                return View(dto);
            }
            try
            {
                await _programApi.UpdateAsync(dto);
                TempData["Success"] = "Proqram uğurla yeniləndi";
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
                await _programApi.DeleteAsync(id);
                TempData["Success"] = "Proqram uğurla silindi";
            }
            catch { TempData["Error"] = "Silmə zamanı xəta baş verdi"; }
            return RedirectToAction("Index");
        }
    }
}
