using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Assignment;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class AssignmentsController : Controller
    {
        private readonly IAssignmentApi _assignmentApi;
        private readonly ICourseOfferingApi _offeringApi;
        private readonly ITeacherApi _teacherApi;

        public AssignmentsController(
            IAssignmentApi assignmentApi,
            ICourseOfferingApi offeringApi,
            ITeacherApi teacherApi)
        {
            _assignmentApi = assignmentApi;
            _offeringApi = offeringApi;
            _teacherApi = teacherApi;
        }

        public async Task<IActionResult> Index(int courseOfferingId)
        {
            var offering = await _offeringApi.GetByIdAsync(courseOfferingId);
            if (offering == null) return NotFound();

            var assignments = await _assignmentApi.GetByCourseAsync(courseOfferingId);
            ViewBag.Offering = offering;
            return View(assignments);
        }

        public async Task<IActionResult> Create(int courseOfferingId)
        {
            var offering = await _offeringApi.GetByIdAsync(courseOfferingId);
            if (offering == null) return NotFound();
            ViewBag.Offering = offering;
            return View(new CreateAssignmentDto { CourseOfferingId = courseOfferingId, DueDate = DateTime.Now.AddDays(7) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAssignmentDto dto)
        {
            try
            {
                await _assignmentApi.CreateAsync(dto);
                TempData["Success"] = "Tapşırıq yaradıldı";
                return RedirectToAction("Index", new { courseOfferingId = dto.CourseOfferingId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var offering = await _offeringApi.GetByIdAsync(dto.CourseOfferingId);
                ViewBag.Offering = offering;
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var assignment = await _assignmentApi.GetByIdAsync(id);
            if (assignment == null) return NotFound();

            var offering = await _offeringApi.GetByIdAsync(assignment.CourseOfferingId);
            ViewBag.Offering = offering;

            return View(new UpdateAssignmentDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                Type = assignment.Type,
                DueDate = assignment.DueDate,
                MaxScore = assignment.MaxScore,
                IsVisible = assignment.IsVisible
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateAssignmentDto dto, int courseOfferingId)
        {
            try
            {
                await _assignmentApi.UpdateAsync(dto);
                TempData["Success"] = "Tapşırıq yeniləndi";
                return RedirectToAction("Index", new { courseOfferingId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int courseOfferingId)
        {
            try
            {
                await _assignmentApi.DeleteAsync(id);
                TempData["Success"] = "Tapşırıq silindi";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index", new { courseOfferingId });
        }

        public async Task<IActionResult> Submissions(int id)
        {
            var assignment = await _assignmentApi.GetByIdAsync(id);
            if (assignment == null) return NotFound();

            var submissions = await _assignmentApi.GetSubmissionsAsync(id);
            ViewBag.Assignment = assignment;
            return View(submissions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(GradeSubmissionDto dto, int courseOfferingId)
        {
            try
            {
                await _assignmentApi.GradeAsync(dto);
                TempData["Success"] = "Qiymət verildi";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Submissions", new { id = HttpContext.Request.Form["assignmentId"] });
        }
    }
}
