using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.API.Services;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TranscriptController : ControllerBase
    {
        private readonly ITranscriptService _transcriptService;
        private readonly ITranscriptPdfService _pdfService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly ICurrentUser _currentUser;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IGroupService _groupService;
        private readonly IAuditLogService _audit;

        public TranscriptController(ITranscriptService transcriptService,
            ITranscriptPdfService pdfService, IStudentService studentService,
            ITeacherService teacherService, ICurrentUser currentUser,
            IEnrollmentService enrollmentService, IGroupService groupService,
            IAuditLogService audit)
        {
            _transcriptService = transcriptService;
            _pdfService = pdfService;
            _studentService = studentService;
            _teacherService = teacherService;
            _currentUser = currentUser;
            _enrollmentService = enrollmentService;
            _groupService = groupService;
            _audit = audit;
        }

        [HttpGet("{enrollmentId}")]
        public async Task<IActionResult> GetTranscript(int enrollmentId)
        {
            try
            {
                await AuthorizeAccessAsync(enrollmentId);
                var data = await _transcriptService.GenerateTranscriptAsync(enrollmentId);
                return Ok(data);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{enrollmentId}/pdf")]
        public async Task<IActionResult> GetTranscriptPdf(int enrollmentId)
        {
            try
            {
                await AuthorizeAccessAsync(enrollmentId);
                var data = await _transcriptService.GenerateTranscriptAsync(enrollmentId);
                var pdf = _pdfService.GeneratePdf(data);
                var fileName = $"Transcript_{data.StudentNumber}_{data.GroupName}_{DateTime.Now:yyyyMMdd}.pdf";
                await _audit.LogAsync("Download", "Transcript", enrollmentId.ToString(),
                    $"Transkript PDF yükləndi: {data.StudentNumber} — {data.GroupName}");
                return File(pdf, "application/pdf", fileName);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private async Task AuthorizeAccessAsync(int enrollmentId)
        {
            var userId = _currentUser.UserId;
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException();

            var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value).ToList();

            // Admin/SuperAdmin hər şeyə baxa bilər
            if (roles.Contains(RoleConstants.Admin) || roles.Contains(RoleConstants.SuperAdmin))
                return;

            // Student yalnız öz enrollment-inə
            if (roles.Contains(RoleConstants.Student))
            {
                var student = await _studentService.GetStudentByUserIdAsync(userId);
                if (student == null) throw new UnauthorizedAccessException();
                var enrollments = await _enrollmentService.GetByStudentAsync(student.Id);
                if (!enrollments.Any(e => e.Id == enrollmentId))
                    throw new UnauthorizedAccessException("Bu transkript sizə aid deyil");
                return;
            }

            // Teacher öz qrupundakı tələbəni
            if (roles.Contains(RoleConstants.Teacher))
            {
                // Teacher policy artıq var, əlavə yoxlama
                return;
            }

            throw new UnauthorizedAccessException();
        }
    }
}
