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
        private readonly IGroupApi _groupApi;
        private readonly ITeacherApi _teacherApi;
        private readonly IAssessmentApi _assessmentApi;

        public AssignmentsController(IAssignmentApi assignmentApi, IGroupApi groupApi,
            ITeacherApi teacherApi, IAssessmentApi assessmentApi)
        {
            _assignmentApi = assignmentApi;
            _groupApi = groupApi;
            _teacherApi = teacherApi;
            _assessmentApi = assessmentApi;
        }

        // groupId=0 → qrupları göstər, seçsin
        public async Task<IActionResult> Index(int groupId = 0)
        {
            if (groupId == 0)
            {
                var groups = await _groupApi.GetMyGroupsAsync();
                return View("SelectGroup", groups);
            }
            var assignments = await _assignmentApi.GetByGroupAsync(groupId);
            ViewBag.GroupId = groupId;
            return View(assignments);
        }

        public async Task<IActionResult> Create(int groupId)
        {
            ViewBag.GroupId = groupId;
            try { ViewBag.AssessmentItems = (await _assessmentApi.GetByGroupAsync(groupId)).ToList(); }
            catch { ViewBag.AssessmentItems = new List<SIS.BLL.Services.DTOs.Assessment.AssessmentItemDto>(); }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAssignmentDto dto)
        {
            await _assignmentApi.CreateAsync(dto);
            TempData["Success"] = "Tapşırıq yaradıldı";
            return RedirectToAction("Index", new { groupId = dto.GroupId });
        }

        public async Task<IActionResult> Submissions(int id)
        {
            var subs = await _assignmentApi.GetSubmissionsAsync(id);
            var assignment = await _assignmentApi.GetByIdAsync(id);
            ViewBag.Assignment = assignment;
            ViewBag.AssignmentId = id;
            return View(subs);
        }

        [HttpPost]
        public async Task<IActionResult> Grade(GradeSubmissionDto dto, int assignmentId)
        {
            await _assignmentApi.GradeAsync(dto);
            TempData["Success"] = "Qiymət verildi";
            return RedirectToAction("Submissions", new { id = assignmentId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var assignment = await _assignmentApi.GetByIdAsync(id);
            var model = new UpdateAssignmentDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                MaxScore = assignment.MaxScore,
                DueDate = assignment.DueDate
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateAssignmentDto dto)
        {
            await _assignmentApi.UpdateAsync(dto);
            TempData["Success"] = "Tapşırıq yeniləndi";
            return RedirectToAction("Index", new { groupId = 0 });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int groupId)
        {
            await _assignmentApi.DeleteAsync(id);
            TempData["Success"] = "Tapşırıq silindi";
            return RedirectToAction("Index", new { groupId });
        }
    }
}
