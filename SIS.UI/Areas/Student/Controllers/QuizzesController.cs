using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Enrollment;
using SIS.BLL.Services.DTOs.Quiz;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Policy = "StudentOnly")]
    public class QuizzesController : Controller
    {
        private readonly IQuizApi _quizApi;
        private readonly IEnrollmentApi _enrollmentApi;

        public QuizzesController(IQuizApi quizApi, IEnrollmentApi enrollmentApi)
        {
            _quizApi = quizApi;
            _enrollmentApi = enrollmentApi;
        }

        public async Task<IActionResult> Index()
        {
            var enrollments = (await _enrollmentApi.GetMyEnrollmentsAsync()).ToList();
            var result = new List<(EnrollmentListDto enrollment, QuizDto quiz, QuizResultDto? myResult)>();

            foreach (var e in enrollments)
            {
                try
                {
                    var quizzes = await _quizApi.GetActiveByGroupAsync(e.GroupId);
                    foreach (var q in quizzes)
                    {
                        QuizResultDto? my = null;
                        try { my = await _quizApi.GetMyResultAsync(q.Id, e.Id); } catch { }
                        result.Add((e, q, my));
                    }
                }
                catch { }
            }

            return View(result);
        }

        public async Task<IActionResult> Take(int quizId, int enrollmentId)
        {
            TakeQuizDto take;
            try
            {
                take = await _quizApi.StartAsync(quizId, enrollmentId);
            }
            catch
            {
                TempData["Error"] = "Quiz açıla bilmədi (aktiv deyil və ya sizə aid deyil)";
                return RedirectToAction("Index");
            }
            return View(take);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int quizId, int enrollmentId)
        {
            // Form açarları: q_{questionId} = optionId (radio) və ya bir neçə optionId (checkbox)
            var answers = new List<SubmitAnswerDto>();
            foreach (var key in Request.Form.Keys)
            {
                if (!key.StartsWith("q_")) continue;
                if (!int.TryParse(key.Substring(2), out var questionId)) continue;

                var optionIds = Request.Form[key]
                    .SelectMany(v => (v ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(v => int.TryParse(v, out var id) ? id : 0)
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                answers.Add(new SubmitAnswerDto { QuizQuestionId = questionId, SelectedOptionIds = optionIds });
            }

            try
            {
                var res = await _quizApi.SubmitAsync(new SubmitQuizDto
                {
                    QuizId = quizId,
                    EnrollmentId = enrollmentId,
                    Answers = answers
                });
                TempData["Success"] = $"Quiz təhvil verildi — nəticə: {res.Score}%";
            }
            catch
            {
                TempData["Error"] = "Quiz təhvil verilə bilmədi (artıq cavablandırmısınız və ya aktiv deyil)";
            }
            return RedirectToAction("Index");
        }
    }
}
