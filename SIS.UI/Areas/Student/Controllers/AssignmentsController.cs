using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Assignment;
using SIS.BLL.Services.DTOs.Enrollment;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Policy = "StudentOnly")]
    public class AssignmentsController : Controller
    {
        private readonly IAssignmentApi _assignmentApi;
        private readonly IStudentApi _studentApi;
        private readonly IEnrollmentApi _enrollmentApi;

        public AssignmentsController(IAssignmentApi assignmentApi, IStudentApi studentApi, IEnrollmentApi enrollmentApi)
        {
            _assignmentApi = assignmentApi;
            _studentApi = studentApi;
            _enrollmentApi = enrollmentApi;
        }

        public async Task<IActionResult> Index()
        {
            var enrollments = (await _enrollmentApi.GetMyEnrollmentsAsync()).ToList();

            var result = new List<(EnrollmentListDto enrollment, AssignmentDto assignment)>();
            foreach (var e in enrollments)
            {
                try
                {
                    var assignments = await _assignmentApi.GetByGroupAsync(e.GroupId);
                    foreach (var a in assignments)
                        result.Add((e, a));
                }
                catch { }
            }

            return View(result);
        }

        public async Task<IActionResult> Course(int groupId, int enrollmentId)
        {
            // enrollmentId-dən qrup tapırıq
            if (groupId == 0)
            {
                var enrollments = await _enrollmentApi.GetMyEnrollmentsAsync();
                var match = enrollments.FirstOrDefault(e => e.Id == enrollmentId);
                if (match != null) groupId = match.GroupId;
            }

            var allEnrollments = await _enrollmentApi.GetMyEnrollmentsAsync();
            var enrollment = allEnrollments.FirstOrDefault(e => e.Id == enrollmentId);

            var assignments = await _assignmentApi.GetByGroupAsync(groupId);
            var mySubmissions = (await _assignmentApi.GetMySubmissionsAsync(enrollmentId)).ToList();

            // SubmissionMap: assignmentId → submission
            var submissionMap = new Dictionary<int, SubmissionDto>();
            foreach (var s in mySubmissions)
                submissionMap[s.AssignmentId] = s;

            ViewBag.Enrollment = enrollment;
            ViewBag.EnrollmentId = enrollmentId;
            ViewBag.SubmissionMap = submissionMap;
            return View(assignments);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(CreateSubmissionDto dto)
        {
            await _assignmentApi.SubmitAsync(dto);
            TempData["Success"] = "Tapşırıq göndərildi";

            // Enrollment-dən groupId-ni tapırıq
            var enrollments = await _enrollmentApi.GetMyEnrollmentsAsync();
            var enrollment = enrollments.FirstOrDefault(e => e.Id == dto.EnrollmentId);
            var groupId = enrollment?.GroupId ?? 0;

            return RedirectToAction("Course", new { groupId, enrollmentId = dto.EnrollmentId });
        }
    }
}
