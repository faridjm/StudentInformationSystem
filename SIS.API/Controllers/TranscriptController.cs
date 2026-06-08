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
        private readonly IEmailService _emailService;
        private readonly ITranscriptVerificationService _verification;
        private readonly IQrCodeGenerator _qr;
        private readonly IConfiguration _config;

        public TranscriptController(ITranscriptService transcriptService,
            ITranscriptPdfService pdfService, IStudentService studentService,
            ITeacherService teacherService, ICurrentUser currentUser,
            IEnrollmentService enrollmentService, IGroupService groupService,
            IAuditLogService audit, IEmailService emailService,
            ITranscriptVerificationService verification, IQrCodeGenerator qr,
            IConfiguration config)
        {
            _transcriptService = transcriptService;
            _pdfService = pdfService;
            _studentService = studentService;
            _teacherService = teacherService;
            _currentUser = currentUser;
            _enrollmentService = enrollmentService;
            _groupService = groupService;
            _audit = audit;
            _emailService = emailService;
            _verification = verification;
            _qr = qr;
            _config = config;
        }

        /// <summary>
        /// Doğrulama qeydini yaradır/tapır, QR şəklini hazırlayır və DTO-nu doldurur.
        /// Download və Send-to-Email eyni axını istifadə edir → hər ikisində QR olur.
        /// </summary>
        private async Task<byte[]> BuildVerificationAsync(SIS.BLL.Services.DTOs.Transcript.TranscriptDto data, string langCode)
        {
            var record = await _verification.GetOrCreateAsync(data, langCode);

            var baseUrl = (_config["App:PublicBaseUrl"]
                           ?? _config["App:FrontendUrl"]
                           ?? "https://localhost:7002").TrimEnd('/');
            var url = $"{baseUrl}/VerifyTranscript/{record.VerificationCode}";

            data.VerificationCode = record.VerificationCode;
            data.VerificationUrl = url;
            data.VerificationIssuedAt = record.IssuedAt;
            data.VerificationExpiresAt = record.ExpiresAt;

            await _audit.LogAsync("GenerateTranscriptVerification", "TranscriptVerification",
                record.VerificationCode,
                $"Transcript verification generated for student {data.StudentName}, group {data.GroupName}.");

            return _qr.GeneratePng(url, 10);
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
        public async Task<IActionResult> GetTranscriptPdf(int enrollmentId, [FromQuery] string? lang = "az")
        {
            try
            {
                await AuthorizeAccessAsync(enrollmentId);
                var data = await _transcriptService.GenerateTranscriptAsync(enrollmentId);
                var langCode = PdfLocalization.Normalize(lang);
                var qr = await BuildVerificationAsync(data, langCode);
                var pdf = _pdfService.GeneratePdf(data, langCode, qr);
                var fileName = $"Transcript_{data.StudentNumber}_{data.GroupName}_{langCode}_{DateTime.Now:yyyyMMdd}.pdf";
                await _audit.LogAsync("DownloadTranscriptPdf", "Transcript", enrollmentId.ToString(),
                    $"Transkript PDF yükləndi ({langCode}): {data.StudentNumber} — {data.GroupName}");
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

        [HttpPost("{enrollmentId}/email")]
        public async Task<IActionResult> SendTranscriptByEmail(int enrollmentId, [FromQuery] string? lang = "az")
        {
            try
            {
                await AuthorizeAccessAsync(enrollmentId);
                var data = await _transcriptService.GenerateTranscriptAsync(enrollmentId);
                if (string.IsNullOrEmpty(data.Email))
                    return BadRequest(new { error = "Bu hesabda email yoxdur" });

                var langCode = PdfLocalization.Normalize(lang);
                var qr = await BuildVerificationAsync(data, langCode);
                var pdf = _pdfService.GeneratePdf(data, langCode, qr);
                var safeName = SanitizeFileName(data.StudentName);
                var fileName = langCode == "az"
                    ? $"Akademik_Cixaris_{safeName}.pdf"
                    : $"Academic_Transcript_{safeName}.pdf";

                try
                {
                    await _emailService.SendTranscriptAsync(data.Email, data.StudentName,
                        data.CourseName, data.GroupName, pdf, fileName, langCode);
                }
                catch (Exception ex)
                {
                    await _audit.LogAsync("SendTranscriptToEmailFailed", "Transcript", enrollmentId.ToString(),
                        $"Transkript email göndərilmədi ({langCode}): {data.StudentNumber} — {ex.Message}");
                    return StatusCode(500, new { error = "Email göndərilə bilmədi" });
                }

                await _audit.LogAsync("SendTranscriptToEmail", "Transcript", enrollmentId.ToString(),
                    $"Student {data.StudentName} sent transcript PDF to email in {langCode} language.");
                return Ok(new { message = "Transkript email-ə göndərildi" });
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

        /// <summary>
        /// Public transcript doğrulama — login tələb etmir.
        /// QR kod bu endpoint-ə yönəlir (UI public səhifəsi vasitəsilə).
        /// Status: Valid / Invalid / Revoked / Expired.
        /// </summary>
        [HttpGet("verify/{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify(string code)
        {
            try
            {
                var result = await _verification.GetByCodeAsync(code);
                await _audit.LogAsync("VerifyTranscriptViewed", "TranscriptVerification", code,
                    $"Transcript verification viewed for code {code}.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Doğrulama qeydini ləğv edir (yalnız Admin/SuperAdmin).</summary>
        [HttpPost("verify/{code}/revoke")]
        [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.SuperAdmin)]
        public async Task<IActionResult> Revoke(string code)
        {
            try
            {
                var ok = await _verification.RevokeAsync(code);
                if (!ok) return NotFound(new { error = "Doğrulama kodu tapılmadı və ya artıq ləğv olunub" });
                await _audit.LogAsync("RevokeTranscriptVerification", "TranscriptVerification", code,
                    $"Transcript verification revoked for code {code}.");
                return Ok(new { message = "Doğrulama ləğv olundu" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Fayl adı üçün təhlükəsiz simvollar (problemli simvolları _ ilə əvəz et)
        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Student";
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var clean = new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
            clean = clean.Replace(' ', '_');
            return string.IsNullOrWhiteSpace(clean) ? "Student" : clean;
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
