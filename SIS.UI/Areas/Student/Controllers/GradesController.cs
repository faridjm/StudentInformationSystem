using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Policy = "StudentOnly")]
    public class GradesController : Controller
    {
        private readonly IEnrollmentApi _enrollmentApi;
        private readonly IAssessmentApi _assessmentApi;
        private readonly IAssignmentApi _assignmentApi;
        private readonly IStudentApi _studentApi;

        public GradesController(IEnrollmentApi enrollmentApi, IAssessmentApi assessmentApi,
            IAssignmentApi assignmentApi, IStudentApi studentApi)
        {
            _enrollmentApi = enrollmentApi;
            _assessmentApi = assessmentApi;
            _assignmentApi = assignmentApi;
            _studentApi = studentApi;
        }

        public async Task<IActionResult> Index()
        {
            var enrollments = await _enrollmentApi.GetMyEnrollmentsAsync();
            var enrollmentList = enrollments.ToList();

            // Rəsmi qiymətləndirmə (OfficialGrade hesabı üçün)
            var submissionsPerEnrollment = new Dictionary<int, IEnumerable<SIS.BLL.Services.DTOs.Assessment.AssessmentSubmissionDto>>();
            // Ev tapşırıqları (AssignmentAverage üçün)
            var assignmentSubsPerEnrollment = new Dictionary<int, IEnumerable<SIS.BLL.Services.DTOs.Assignment.SubmissionDto>>();

            foreach (var e in enrollmentList)
            {
                try { submissionsPerEnrollment[e.Id] = await _assessmentApi.GetMySubmissionsAsync(e.Id); }
                catch { submissionsPerEnrollment[e.Id] = []; }

                try { assignmentSubsPerEnrollment[e.Id] = await _assignmentApi.GetMySubmissionsAsync(e.Id); }
                catch { assignmentSubsPerEnrollment[e.Id] = []; }
            }

            ViewBag.Enrollments = enrollmentList;
            ViewBag.SubmissionsPerEnrollment = submissionsPerEnrollment;
            ViewBag.AssignmentSubsPerEnrollment = assignmentSubsPerEnrollment;
            return View();
        }
    }
}
