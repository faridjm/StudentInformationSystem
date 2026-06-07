using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Quiz;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;

namespace SIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuizController : ControllerBase
    {
        private const string TeacherRoles = $"{RoleConstants.Admin},{RoleConstants.SuperAdmin},{RoleConstants.Teacher}";

        private readonly IQuizService _service;
        public QuizController(IQuizService service) { _service = service; }

        // ── Read ──────────────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var q = await _service.GetByIdAsync(id);
            return q == null ? NotFound() : Ok(q);
        }

        [HttpGet("{id}/detail")]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> GetDetail(int id)
        {
            var q = await _service.GetDetailAsync(id);
            return q == null ? NotFound() : Ok(q);
        }

        [HttpGet("by-assessment/{assessmentItemId}")]
        public async Task<IActionResult> GetByAssessment(int assessmentItemId)
        {
            var q = await _service.GetByAssessmentItemAsync(assessmentItemId);
            return q == null ? NotFound() : Ok(q);
        }

        [HttpGet("group/{groupId}")]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> GetByGroup(int groupId)
            => Ok(await _service.GetByGroupAsync(groupId));

        [HttpGet("group/{groupId}/active")]
        public async Task<IActionResult> GetActiveByGroup(int groupId)
            => Ok(await _service.GetActiveByGroupAsync(groupId));

        [HttpGet("{id}/results")]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> GetResults(int id)
            => Ok(await _service.GetResultsAsync(id));

        /// <summary>Tək cəhd üçün sual-sual detallı nəticə (Teacher və ya owning Student).</summary>
        [HttpGet("attempts/{attemptId}/detail")]
        public async Task<IActionResult> GetAttemptDetail(int attemptId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException();
            var dto = await _service.GetAttemptDetailAsync(attemptId, userId);
            return dto == null ? NotFound() : Ok(dto);
        }

        // ── Teacher: manage ────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> Create([FromBody] CreateQuizDto dto)
            => Ok(await _service.CreateAsync(dto));

        [HttpPut]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> Update([FromBody] UpdateQuizDto dto)
        {
            await _service.UpdateAsync(dto);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok();
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> SetActive(int id, [FromQuery] bool active)
        {
            await _service.SetActiveAsync(id, active);
            return Ok();
        }

        [HttpPost("{id}/questions")]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> AddQuestion(int id, [FromBody] CreateQuizQuestionDto dto)
            => Ok(await _service.AddQuestionAsync(id, dto));

        [HttpDelete("{quizId}/questions/{questionId}")]
        [Authorize(Roles = TeacherRoles)]
        public async Task<IActionResult> DeleteQuestion(int quizId, int questionId)
        {
            await _service.DeleteQuestionAsync(quizId, questionId);
            return Ok();
        }

        // ── Student: take ──────────────────────────────────────────────
        [HttpGet("{quizId}/start/{enrollmentId}")]
        [Authorize(Roles = RoleConstants.Student)]
        public async Task<IActionResult> Start(int quizId, int enrollmentId)
            => Ok(await _service.StartAttemptAsync(quizId, enrollmentId));

        [HttpPost("submit")]
        [Authorize(Roles = RoleConstants.Student)]
        public async Task<IActionResult> Submit([FromBody] SubmitQuizDto dto)
            => Ok(await _service.SubmitAttemptAsync(dto));

        [HttpGet("{quizId}/my-result/{enrollmentId}")]
        public async Task<IActionResult> GetMyResult(int quizId, int enrollmentId)
        {
            var r = await _service.GetMyResultAsync(quizId, enrollmentId);
            return r == null ? NotFound() : Ok(r);
        }
    }
}
