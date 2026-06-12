using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.API.Services;
using SIS.BLL.Services.DTOs.Parent;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    /// <summary>
    /// Valideyn paneli — READ-ONLY. Valideyn yalnız aktiv əlaqəli övladının
    /// akademik göstəricilərini görə bilər. Heç bir məlumatı dəyişə bilməz.
    /// Bütün endpoint-lərdə parent-student access yoxlanır.
    /// </summary>
    [ApiController]
    [Route("api/parent")]
    [Authorize(Roles = RoleConstants.Parent)]
    public class ParentController : ControllerBase
    {
        private readonly ICurrentUser _currentUser;
        private readonly IParentService _parentService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly ITranscriptService _transcriptService;
        private readonly IAnnouncementService _announcementService;
        private readonly IQuizService _quizService;
        private readonly IAuditLogService _audit;

        // Transcript PDF (mövcud transcript pipeline ilə eyni)
        private readonly ITranscriptPdfService _pdfService;
        private readonly ITranscriptVerificationService _verification;
        private readonly IQrCodeGenerator _qr;
        private readonly IConfiguration _config;

        public ParentController(ICurrentUser currentUser, IParentService parentService,
            IEnrollmentService enrollmentService, ITranscriptService transcriptService,
            IAnnouncementService announcementService, IQuizService quizService,
            IAuditLogService audit, ITranscriptPdfService pdfService,
            ITranscriptVerificationService verification, IQrCodeGenerator qr, IConfiguration config)
        {
            _currentUser = currentUser;
            _parentService = parentService;
            _enrollmentService = enrollmentService;
            _transcriptService = transcriptService;
            _announcementService = announcementService;
            _quizService = quizService;
            _audit = audit;
            _pdfService = pdfService;
            _verification = verification;
            _qr = qr;
            _config = config;
        }

        private string ParentId => _currentUser.UserId ?? "";

        [HttpGet("children")]
        public async Task<IActionResult> GetChildren()
            => Ok(await _parentService.GetChildrenAsync(ParentId));

        [HttpGet("children/{studentId}/enrollments")]
        public async Task<IActionResult> GetEnrollments(int studentId)
        {
            try
            {
                await _parentService.EnsureParentCanAccessAsync(ParentId, studentId);
                return Ok(await _enrollmentService.GetByStudentAsync(studentId));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        [HttpGet("children/{studentId}/transcript/{enrollmentId}")]
        public async Task<IActionResult> GetTranscript(int studentId, int enrollmentId)
        {
            try
            {
                await _parentService.EnsureParentCanAccessAsync(ParentId, studentId);
                if (!await EnrollmentBelongsToStudent(enrollmentId, studentId)) return NotFound();

                await _audit.LogAsync("ParentViewDashboard", "ParentStudent", studentId.ToString(),
                    $"Parent viewed academic data for student {studentId}.");
                return Ok(await _transcriptService.GenerateTranscriptAsync(enrollmentId));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpGet("children/{studentId}/announcements")]
        public async Task<IActionResult> GetAnnouncements(int studentId)
        {
            try
            {
                await _parentService.EnsureParentCanAccessAsync(ParentId, studentId);
                return Ok(await _announcementService.GetForStudentAsync(studentId));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        [HttpGet("children/{studentId}/quiz-results")]
        public async Task<IActionResult> GetQuizResults(int studentId)
        {
            try
            {
                await _parentService.EnsureParentCanAccessAsync(ParentId, studentId);

                var enrollments = await _enrollmentService.GetByStudentAsync(studentId);
                var results = new List<ParentQuizResultDto>();
                foreach (var e in enrollments)
                {
                    IEnumerable<SIS.BLL.Services.DTOs.Quiz.QuizDto> quizzes;
                    try { quizzes = await _quizService.GetByGroupAsync(e.GroupId); }
                    catch { continue; }

                    foreach (var q in quizzes)
                    {
                        SIS.BLL.Services.DTOs.Quiz.QuizResultDto? r = null;
                        try { r = await _quizService.GetMyResultAsync(q.Id, e.Id); } catch { }

                        results.Add(new ParentQuizResultDto
                        {
                            QuizTitle = q.Title,
                            GroupName = e.GroupName,
                            CourseName = e.CourseName,
                            MaxScore = q.MaxScore,
                            Score = r?.Score,
                            IsSubmitted = r?.IsSubmitted ?? false,
                            SubmittedAt = r?.SubmittedAt,
                            Percentage = (r?.Score.HasValue == true && q.MaxScore > 0)
                                ? Math.Round(r.Score!.Value / q.MaxScore * 100, 1) : null
                        });
                    }
                }
                return Ok(results);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        [HttpGet("children/{studentId}/transcript/{enrollmentId}/pdf")]
        public async Task<IActionResult> GetTranscriptPdf(int studentId, int enrollmentId, [FromQuery] string? lang = "az")
        {
            try
            {
                await _parentService.EnsureParentCanAccessAsync(ParentId, studentId);
                if (!await EnrollmentBelongsToStudent(enrollmentId, studentId)) return NotFound();

                var data = await _transcriptService.GenerateTranscriptAsync(enrollmentId);
                var langCode = PdfLocalization.Normalize(lang);

                var record = await _verification.GetOrCreateAsync(data, langCode);
                var baseUrl = (_config["App:PublicBaseUrl"] ?? _config["App:FrontendUrl"] ?? "https://localhost:7002").TrimEnd('/');
                var url = $"{baseUrl}/VerifyTranscript/{record.VerificationCode}";
                data.VerificationCode = record.VerificationCode;
                data.VerificationUrl = url;
                data.VerificationIssuedAt = record.IssuedAt;
                var qr = _qr.GeneratePng(url, 10);

                var pdf = _pdfService.GeneratePdf(data, langCode, qr);
                await _audit.LogAsync("ParentDownloadTranscript", "Transcript", enrollmentId.ToString(),
                    $"Parent downloaded transcript ({langCode}) for student {studentId}.");

                var fileName = $"Transcript_{data.StudentNumber}_{langCode}.pdf";
                return File(pdf, "application/pdf", fileName);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        private async Task<bool> EnrollmentBelongsToStudent(int enrollmentId, int studentId)
        {
            var e = await _enrollmentService.GetByIdAsync(enrollmentId);
            return e != null && e.StudentId == studentId;
        }
    }
}
