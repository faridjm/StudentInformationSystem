using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Auth;
using SIS.BLL.Services.DTOs.Student;
using SIS.UI.ApiClients;
using SIS.UI.ViewModels;

namespace SIS.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class StudentController : Controller
    {
        private readonly IStudentApi _studentApi;
        private readonly IAuthApi _authApi;
        private readonly IInstitutionalEmailApi _emailApi;
        private readonly IEnrollmentApi _enrollmentApi;
        private readonly ITranscriptApi _transcriptApi;

        public StudentController(IStudentApi studentApi, IAuthApi authApi,
            IInstitutionalEmailApi emailApi, IEnrollmentApi enrollmentApi,
            ITranscriptApi transcriptApi)
        {
            _studentApi = studentApi;
            _authApi    = authApi;
            _emailApi   = emailApi;
            _enrollmentApi = enrollmentApi;
            _transcriptApi = transcriptApi;
        }

        public async Task<IActionResult> Index()
            => View(await _studentApi.GetAllAsync());

        public async Task<IActionResult> Detail(int id)
        {
            var s = await _studentApi.GetByIdAsync(id);

            // F1 — Academic Information: tələbənin enrollment-ləri + akademik xülasə.
            // Data mövcud Enrollment + Transcript servislərindən gəlir (schema dəyişikliyi yoxdur).
            var rows = new List<StudentAcademicRowVm>();
            try
            {
                var enrollments = await _enrollmentApi.GetByStudentAsync(id);
                foreach (var e in enrollments)
                {
                    var row = new StudentAcademicRowVm
                    {
                        EnrollmentId = e.Id,
                        CourseName   = e.CourseName,
                        GroupName    = e.GroupName,
                        TeacherName  = e.TeacherName,
                        Status       = e.Status,
                        EnrolledAt   = e.EnrolledAt
                    };
                    // Akademik göstəricilər (attendance %, official grade, GPA, season, tarixlər)
                    // transcript servisindən gəlir. Hesablama yoxdursa sətir yenə də göstərilir.
                    try
                    {
                        var t = await _transcriptApi.GetAsync(e.Id);
                        row.AcademicYear         = t.AcademicYear;
                        row.StartDate            = t.StartDate;
                        row.EndDate              = t.EndDate;
                        // Yalnız davamiyyət qeyd olunmuşsa faiz göstər
                        var hasAtt               = (t.PresentCount + t.LateCount + t.AbsentCount) > 0;
                        row.AttendancePercentage = hasAtt ? (double?)t.AttendancePercentage : null;
                        row.OfficialGrade        = t.OfficialGrade;
                        row.GPA                  = t.GPA;
                        row.CertificateEligible  = t.CertificateEligible;
                        row.TranscriptLoaded     = true;
                    }
                    catch { /* transcript hazır deyil — sətir əsas məlumatla göstərilir */ }

                    rows.Add(row);
                }
            }
            catch { /* enrollment servisi əlçatan deyil — boş akademik blok */ }

            ViewBag.AcademicRows = rows;
            return View(s);
        }

        // F1/F4 — Tələbənin transkript PDF-ni yüklə (API-ni proxy edir, Admin icazəlidir).
        public async Task<IActionResult> Transcript(int enrollmentId, int studentId)
        {
            var response = await _transcriptApi.GetPdfAsync(enrollmentId);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Transkript yüklənə bilmədi";
                return RedirectToAction("Detail", new { id = studentId });
            }
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "transcript.pdf";
            return File(bytes, "application/pdf", fileName);
        }

        public IActionResult Create() => View();

        /// <summary>
        /// AJAX: /Admin/Student/GenerateEmail?firstName=Farid&amp;lastName=Mammadov
        /// → { "email": "faridm1@sis.az" }
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateEmail(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return BadRequest(new { error = "Ad və soyad tələb olunur" });
            try
            {
                var result = await _emailApi.GenerateAsync(firstName, lastName);
                return Json(new { email = result.Email });
            }
            catch
            {
                return StatusCode(500, new { error = "Email generate edilmədi" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateStudentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _studentApi.CreateAsync(dto);
                TempData["Success"] = "Tələbə yaradıldı";
                return RedirectToAction("Index");
            }
            catch (Refit.ApiException apiEx)
            {
                TempData["Error"] = await ExtractApiError(apiEx);
                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var s = await _studentApi.GetByIdAsync(id);
            return View(new UpdateStudentDto { Id = s.Id, FullName = s.FullName, FinCode = s.FinCode, PhoneNumber = s.PhoneNumber });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateStudentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _studentApi.UpdateAsync(dto);
            TempData["Success"] = "Dəyişikliklər saxlandı";
            return RedirectToAction("Detail", new { id = dto.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _studentApi.DeleteAsync(id);
            TempData["Success"] = "Tələbə silindi";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SetPassword(string userId, string newPassword, string confirmPassword, int studentId)
        {
            if (newPassword != confirmPassword) { TempData["Error"] = "Şifrələr uyğun deyil"; return RedirectToAction("Detail", new { id = studentId }); }
            try { await _authApi.AdminSetPasswordAsync(new AdminSetPasswordDto { UserId = userId, NewPassword = newPassword, ConfirmPassword = confirmPassword }); TempData["Success"] = "Şifrə dəyişdirildi"; }
            catch { TempData["Error"] = "Xəta baş verdi"; }
            return RedirectToAction("Detail", new { id = studentId });
        }

        private static async Task<string> ExtractApiError(Refit.ApiException apiEx)
        {
            try
            {
                var content = await apiEx.GetContentAsAsync<System.Text.Json.JsonElement>();
                if (content.TryGetProperty("message", out var msg)) return msg.GetString() ?? apiEx.ReasonPhrase ?? "Xəta";
                if (content.TryGetProperty("errors", out var errs))
                    return string.Join("; ", errs.EnumerateArray().Select(e => e.GetProperty("error").GetString()));
            }
            catch { }
            return apiEx.ReasonPhrase ?? "Xəta baş verdi";
        }
    }
}
