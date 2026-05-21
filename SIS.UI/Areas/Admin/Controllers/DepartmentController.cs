using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Department;
using SIS.Common.Pagination;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentApi _departmentApi;

        public DepartmentController(IDepartmentApi departmentApi)
        {
            _departmentApi = departmentApi;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var request = new PagedRequest { Page = page, PageSize = 10, SearchTerm = search };
            var result = await _departmentApi.GetAllAsync(request);
            ViewBag.SearchTerm = search;
            return View(result);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateDepartmentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _departmentApi.CreateAsync(dto);
                TempData["Success"] = "Departament uğurla yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dept = await _departmentApi.GetByIdAsync(id);
            if (dept == null) return NotFound();
            var dto = new UpdateDepartmentDto { Id = dept.Id, Name = dept.Name, Code = dept.Code, Description = dept.Description };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateDepartmentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _departmentApi.UpdateAsync(dto);
                TempData["Success"] = "Departament uğurla yeniləndi";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _departmentApi.DeleteAsync(id);
                TempData["Success"] = "Departament uğurla silindi";
            }
            catch { TempData["Error"] = "Silmə zamanı xəta baş verdi"; }
            return RedirectToAction("Index");
        }
    }
}
