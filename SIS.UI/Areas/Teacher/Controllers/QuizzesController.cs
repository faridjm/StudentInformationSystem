using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Assessment;
using SIS.BLL.Services.DTOs.Quiz;
using SIS.Entities.Enums;
using SIS.UI.ApiClients;

namespace SIS.UI.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Policy = "TeacherOnly")]
    public class QuizzesController : Controller
    {
        private readonly IQuizApi _quizApi;
        private readonly IGroupApi _groupApi;
        private readonly IAssessmentApi _assessmentApi;

        public QuizzesController(IQuizApi quizApi, IGroupApi groupApi, IAssessmentApi assessmentApi)
        {
            _quizApi = quizApi;
            _groupApi = groupApi;
            _assessmentApi = assessmentApi;
        }

        // groupId=0 → qrupları göstər
        public async Task<IActionResult> Index(int groupId = 0)
        {
            if (groupId == 0)
            {
                var groups = await _groupApi.GetMyGroupsAsync();
                return View("SelectGroup", groups);
            }

            var quizzes = (await _quizApi.GetByGroupAsync(groupId)).ToList();

            // Quiz tipli, hələ quiz-i olmayan AssessmentItem-lər (yeni quiz yaratmaq üçün)
            var usedItemIds = quizzes.Select(q => q.AssessmentItemId).ToHashSet();
            var quizItems = new List<AssessmentItemDto>();
            try
            {
                quizItems = (await _assessmentApi.GetByGroupAsync(groupId))
                    .Where(a => a.Type == AssessmentType.Quiz && !usedItemIds.Contains(a.Id))
                    .ToList();
            }
            catch { }

            ViewBag.GroupId = groupId;
            ViewBag.AvailableItems = quizItems;
            return View(quizzes);
        }

        public async Task<IActionResult> Create(int groupId, int assessmentItemId)
        {
            ViewBag.GroupId = groupId;
            AssessmentItemDto? item = null;
            try
            {
                item = (await _assessmentApi.GetByGroupAsync(groupId))
                    .FirstOrDefault(a => a.Id == assessmentItemId && a.Type == AssessmentType.Quiz);
            }
            catch { }

            if (item == null)
            {
                TempData["Error"] = "Quiz tipli qiymətləndirmə elementi tapılmadı";
                return RedirectToAction("Index", new { groupId });
            }

            ViewBag.Item = item;
            return View(new CreateQuizDto { AssessmentItemId = item.Id, Title = item.Title });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateQuizDto dto, int groupId)
        {
            try
            {
                var quiz = await _quizApi.CreateAsync(dto);
                TempData["Success"] = "Quiz yaradıldı — indi sualları əlavə edin";
                return RedirectToAction("Manage", new { id = quiz.Id });
            }
            catch
            {
                TempData["Error"] = "Quiz yaradıla bilmədi (bu element üçün artıq quiz ola bilər)";
                return RedirectToAction("Index", new { groupId });
            }
        }

        public async Task<IActionResult> Manage(int id)
        {
            var quiz = await _quizApi.GetDetailAsync(id);
            return View(quiz);
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestion(int quizId, string Text, int Type, decimal Points,
            string[] OptionTexts, int[] CorrectIndexes)
        {
            var options = new List<CreateQuizOptionDto>();
            for (int i = 0; i < (OptionTexts?.Length ?? 0); i++)
            {
                if (string.IsNullOrWhiteSpace(OptionTexts![i])) continue;
                options.Add(new CreateQuizOptionDto
                {
                    Text = OptionTexts[i].Trim(),
                    IsCorrect = CorrectIndexes != null && CorrectIndexes.Contains(i)
                });
            }

            try
            {
                await _quizApi.AddQuestionAsync(quizId, new CreateQuizQuestionDto
                {
                    Text = Text,
                    Type = (QuestionType)Type,
                    Points = Points <= 0 ? 1 : Points,
                    Options = options
                });
                TempData["Success"] = "Sual əlavə edildi";
            }
            catch
            {
                TempData["Error"] = "Sual əlavə edilə bilmədi (variantları və düz cavabı yoxlayın)";
            }
            return RedirectToAction("Manage", new { id = quizId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int quizId, int questionId)
        {
            try { await _quizApi.DeleteQuestionAsync(quizId, questionId); TempData["Success"] = "Sual silindi"; }
            catch { TempData["Error"] = "Sual silinə bilmədi"; }
            return RedirectToAction("Manage", new { id = quizId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id, bool active)
        {
            try
            {
                await _quizApi.SetActiveAsync(id, active);
                TempData["Success"] = active ? "Quiz aktivləşdirildi" : "Quiz dayandırıldı";
            }
            catch
            {
                TempData["Error"] = "Status dəyişdirilə bilmədi (aktivləşdirmək üçün ən azı bir sual lazımdır)";
            }
            return RedirectToAction("Manage", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int groupId)
        {
            try { await _quizApi.DeleteAsync(id); TempData["Success"] = "Quiz silindi"; }
            catch { TempData["Error"] = "Quiz silinə bilmədi"; }
            return RedirectToAction("Index", new { groupId });
        }

        public async Task<IActionResult> Results(int id)
        {
            var quiz = await _quizApi.GetByIdAsync(id);
            var results = await _quizApi.GetResultsAsync(id);
            ViewBag.Quiz = quiz;
            return View(results);
        }

        /// <summary>Tək cəhd üçün sual-sual detallı nəticə (Teacher görüşü).</summary>
        public async Task<IActionResult> AttemptDetail(int attemptId)
        {
            try
            {
                var dto = await _quizApi.GetAttemptDetailAsync(attemptId);
                return View(dto);
            }
            catch (Refit.ApiException)
            {
                TempData["Error"] = "Bu nəticəyə baxmaq icazəniz yoxdur";
                return RedirectToAction("Index");
            }
        }
    }
}
