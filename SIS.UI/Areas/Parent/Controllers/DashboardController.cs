using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.UI.ApiClients;
using SIS.UI.ViewModels;

namespace SIS.UI.Areas.Parent.Controllers
{
    /// <summary>
    /// Valideyn paneli — READ-ONLY monitoring. Bütün data parent API-dən gəlir və
    /// orada parent-student access yoxlanır. Bu controller heç nə dəyişmir.
    /// </summary>
    [Area("Parent")]
    [Authorize(Policy = "ParentOnly")]
    public class DashboardController : Controller
    {
        private readonly IParentApi _parentApi;

        public DashboardController(IParentApi parentApi)
        {
            _parentApi = parentApi;
        }

        // Ümumi yükləyici: uşaq + enrollment seçimi + transcript snapshot
        private async Task<ParentPageVm> BuildAsync(int? studentId, int? enrollmentId, bool loadTranscript = true)
        {
            var vm = new ParentPageVm();
            vm.Children = (await _parentApi.GetChildrenAsync()).ToList();
            if (vm.Children.Count == 0) return vm;

            var child = studentId.HasValue
                ? vm.Children.FirstOrDefault(c => c.StudentId == studentId.Value)
                : null;
            child ??= vm.Children[0];

            vm.SelectedStudentId = child.StudentId;
            vm.SelectedStudentName = child.FullName;
            vm.SelectedStudentNumber = child.StudentNumber;

            try { vm.Enrollments = (await _parentApi.GetEnrollmentsAsync(child.StudentId)).ToList(); }
            catch { vm.Enrollments = new(); }

            if (vm.Enrollments.Count > 0)
            {
                var enr = enrollmentId.HasValue
                    ? vm.Enrollments.FirstOrDefault(e => e.Id == enrollmentId.Value)
                    : null;
                enr ??= vm.Enrollments[0];
                vm.SelectedEnrollmentId = enr.Id;

                if (loadTranscript)
                {
                    try { vm.Transcript = await _parentApi.GetTranscriptAsync(child.StudentId, enr.Id); }
                    catch { vm.Transcript = null; }
                }
            }

            return vm;
        }

        public async Task<IActionResult> Index(int? studentId, int? enrollmentId)
        {
            var vm = await BuildAsync(studentId, enrollmentId);
            if (vm.HasChildren)
            {
                try { vm.Announcements = (await _parentApi.GetAnnouncementsAsync(vm.SelectedStudentId)).Take(5).ToList(); }
                catch { }
            }
            return View(vm);
        }

        public async Task<IActionResult> Children(int? studentId, int? enrollmentId)
            => View("Index", await BuildAsync(studentId, enrollmentId));

        public async Task<IActionResult> Attendance(int? studentId, int? enrollmentId)
            => View(await BuildAsync(studentId, enrollmentId));

        public async Task<IActionResult> Grades(int? studentId, int? enrollmentId)
            => View(await BuildAsync(studentId, enrollmentId));

        public async Task<IActionResult> Assignments(int? studentId, int? enrollmentId)
            => View(await BuildAsync(studentId, enrollmentId));

        public async Task<IActionResult> Transcript(int? studentId, int? enrollmentId)
            => View(await BuildAsync(studentId, enrollmentId));

        public async Task<IActionResult> Quizzes(int? studentId)
        {
            var vm = await BuildAsync(studentId, null, loadTranscript: false);
            if (vm.HasChildren)
            {
                try { vm.QuizResults = (await _parentApi.GetQuizResultsAsync(vm.SelectedStudentId)).ToList(); }
                catch { }
            }
            return View(vm);
        }

        public async Task<IActionResult> Announcements(int? studentId)
        {
            var vm = await BuildAsync(studentId, null, loadTranscript: false);
            if (vm.HasChildren)
            {
                try { vm.Announcements = (await _parentApi.GetAnnouncementsAsync(vm.SelectedStudentId)).ToList(); }
                catch { }
            }
            return View(vm);
        }

        // Transcript PDF proxy (yalnız bağlı uşaq üçün; access API-də yoxlanır)
        public async Task<IActionResult> TranscriptPdf(int studentId, int enrollmentId, string? lang = "az")
        {
            try
            {
                var response = await _parentApi.GetTranscriptPdfAsync(studentId, enrollmentId, lang);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Transkript yüklənə bilmədi";
                    return RedirectToAction("Transcript", new { studentId, enrollmentId });
                }
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "transcript.pdf";
                return File(bytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Transcript", new { studentId, enrollmentId });
            }
        }
    }
}
