using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Assessment;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class AssessmentsController : Controller
    {
        private readonly IAssessmentApi _assessmentApi;
        private readonly IGroupApi _groupApi;

        public AssessmentsController(IAssessmentApi assessmentApi, IGroupApi groupApi)
        {
            _assessmentApi = assessmentApi;
            _groupApi = groupApi;
        }

        // Müəllimin qruplarının qiymətləndirmə planı
        public async Task<IActionResult> Index()
        {
            var groups = await _groupApi.GetMyGroupsAsync();
            return View(groups);
        }

        // Bir assessment item-in tələbə submissionları
        public async Task<IActionResult> Grade(int id)
        {
            var item = await _assessmentApi.GetByIdAsync(id);
            var submissions = await _assessmentApi.GetSubmissionsAsync(id);
            ViewBag.Item = item;
            return View(submissions.ToList());
        }

        // Qrup üzrə bütün assessment-lər
        [Route("/Teacher/Assessments/GroupAssessments/{groupId}")]
        public async Task<IActionResult> GroupAssessments(int groupId)
        {
            var group = await _groupApi.GetByIdAsync(groupId);
            var assessments = await _assessmentApi.GetByGroupAsync(groupId);
            ViewBag.Group = group;
            return View(assessments.OrderBy(a => a.Date).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> SubmitGrade(GradeAssessmentDto dto, int assessmentItemId)
        {
            try
            {
                await _assessmentApi.GradeAsync(dto);
                TempData["Success"] = "Qiymət yadda saxlandı";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Grade", new { id = assessmentItemId });
        }
    }
}
