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
        private readonly IGroupApi _groupApi;

        public EnrollmentController(IEnrollmentApi enrollmentApi, IStudentApi studentApi, IGroupApi groupApi)
        {
            _enrollmentApi = enrollmentApi;
            _studentApi = studentApi;
            _groupApi = groupApi;
        }

        public async Task<IActionResult> Index()
        {
            var groups = await _groupApi.GetAllAsync();
            return View(groups);
        }

        public async Task<IActionResult> Group(int groupId)
        {
            var enrollments = await _enrollmentApi.GetByGroupAsync(groupId);
            ViewBag.GroupId = groupId;
            return View(enrollments);
        }

        public async Task<IActionResult> Create(int? groupId)
        {
            ViewBag.Students = await _studentApi.GetAllAsync();
            ViewBag.Groups = await _groupApi.GetAllAsync();
            ViewBag.SelectedGroupId = groupId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEnrollmentDto dto)
        {
            try
            {
                await _enrollmentApi.EnrollAsync(dto);
                TempData["Success"] = "Qeydiyyat tamamlandı";
                return RedirectToAction("Group", new { groupId = dto.GroupId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Create", new { groupId = dto.GroupId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Drop(int id, int groupId)
        {
            await _enrollmentApi.DropAsync(id);
            TempData["Success"] = "Qeydiyyat ləğv edildi";
            return RedirectToAction("Group", new { groupId });
        }
    }
}
