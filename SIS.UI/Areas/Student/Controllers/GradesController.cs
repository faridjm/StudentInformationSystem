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
        private readonly ITranscriptApi _transcriptApi;

        public GradesController(IEnrollmentApi enrollmentApi, IAssessmentApi assessmentApi,
            IAssignmentApi assignmentApi, IStudentApi studentApi, ITranscriptApi transcriptApi)
        {
            _enrollmentApi = enrollmentApi;
            _assessmentApi = assessmentApi;
            _assignmentApi = assignmentApi;
            _studentApi = studentApi;
            _transcriptApi = transcriptApi;
        }

        public async Task<IActionResult> Index()
        {
            var enrollments = await _enrollmentApi.GetMyEnrollmentsAsync();
            var enrollmentList = enrollments.ToList();

            var submissionsPerEnrollment = new Dictionary<int, IEnumerable<SIS.BLL.Services.DTOs.Assessment.AssessmentSubmissionDto>>();
            var assignmentSubsPerEnrollment = new Dictionary<int, IEnumerable<SIS.BLL.Services.DTOs.Assignment.SubmissionDto>>();
            var transcriptsPerEnrollment = new Dictionary<int, SIS.BLL.Services.DTOs.Transcript.TranscriptDto>();

            foreach (var e in enrollmentList)
            {
                try { submissionsPerEnrollment[e.Id] = await _assessmentApi.GetMySubmissionsAsync(e.Id); }
                catch { submissionsPerEnrollment[e.Id] = []; }

                try { assignmentSubsPerEnrollment[e.Id] = await _assignmentApi.GetMySubmissionsAsync(e.Id); }
                catch { assignmentSubsPerEnrollment[e.Id] = []; }

                try { transcriptsPerEnrollment[e.Id] = await _transcriptApi.GetAsync(e.Id); }
                catch { /* transcript hazır deyil — view default göstərəcək */ }
            }

            ViewBag.Enrollments = enrollmentList;
            ViewBag.SubmissionsPerEnrollment = submissionsPerEnrollment;
            ViewBag.AssignmentSubsPerEnrollment = assignmentSubsPerEnrollment;
            ViewBag.TranscriptsPerEnrollment = transcriptsPerEnrollment;
            return View();
        }
    }
}
