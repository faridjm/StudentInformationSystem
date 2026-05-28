using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Assignment;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Policy = "StudentOnly")]
    public class AssignmentsController : Controller
    {
        private readonly IAssignmentApi _assignmentApi;
        private readonly IEnrollmentApi _enrollmentApi;
        private readonly ICourseOfferingApi _offeringApi;

        public AssignmentsController(
            IAssignmentApi assignmentApi,
            IEnrollmentApi enrollmentApi,
            ICourseOfferingApi offeringApi)
        {
            _assignmentApi = assignmentApi;
            _enrollmentApi = enrollmentApi;
            _offeringApi = offeringApi;
        }

        // Bütün qeydiyyatlı kursların tapşırıqları
        public async Task<IActionResult> Index()
        {
            var enrollments = (await _enrollmentApi.GetMyEnrollmentsAsync()).ToList();
            var allAssignments = new List<(SIS.BLL.Services.DTOs.Enrollment.EnrollmentListDto enrollment, AssignmentDto assignment)>();

            foreach (var e in enrollments)
            {
                var assignments = await _assignmentApi.GetByCourseAsync(e.CourseOfferingId);
                foreach (var a in assignments.Where(a => a.IsVisible))
                    allAssignments.Add((e, a));
            }

            ViewBag.Enrollments = enrollments;
            return View(allAssignments.OrderBy(x => x.assignment.DueDate).ToList());
        }

        // Bir kursun tapşırıqları + cavab statusu
        public async Task<IActionResult> Course(int enrollmentId)
        {
            var enrollment = (await _enrollmentApi.GetMyEnrollmentsAsync())
                .FirstOrDefault(e => e.Id == enrollmentId);
            if (enrollment == null) return NotFound();

            var assignments = (await _assignmentApi.GetByCourseAsync(enrollment.CourseOfferingId))
                .Where(a => a.IsVisible).ToList();

            var submissions = (await _assignmentApi.GetMySubmissionsAsync(enrollmentId)).ToList();
            var submissionMap = submissions.ToDictionary(s => s.AssignmentId);

            ViewBag.Enrollment = enrollment;
            ViewBag.SubmissionMap = submissionMap;
            return View(assignments);
        }

        // Tapşırığa cavab ver
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int assignmentId, int enrollmentId, string? comment, string? fileUrl)
        {
            try
            {
                await _assignmentApi.SubmitAsync(new CreateSubmissionDto
                {
                    AssignmentId = assignmentId,
                    EnrollmentId = enrollmentId,
                    Comment = comment,
                    FileUrl = fileUrl
                });
                TempData["Success"] = "Cavabınız göndərildi";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Course", new { enrollmentId });
        }
    }
}
