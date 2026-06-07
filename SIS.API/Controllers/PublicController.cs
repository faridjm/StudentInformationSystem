using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIS.BLL.Services.DTOs.Public;
using SIS.BLL.Services.Interfaces;

namespace SIS.API.Controllers
{
    /// <summary>
    /// Login tələb etməyən public marketing endpoint-ləri.
    /// Bütün data mövcud servislərdən kompozisiya olunur (fake data yoxdur).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IGroupService _groupService;
        private readonly ITeacherService _teacherService;
        private readonly ICurriculumTemplateService _curriculumService;
        private readonly IDepartmentService _departmentService;
        private readonly ICourseApplicationService _applicationService;
        private readonly IPublicAdminService _publicAdmin;

        public PublicController(ICourseService courseService, IGroupService groupService,
            ITeacherService teacherService, ICurriculumTemplateService curriculumService,
            IDepartmentService departmentService,
            ICourseApplicationService applicationService,
            IPublicAdminService publicAdmin)
        {
            _courseService = courseService;
            _groupService = groupService;
            _teacherService = teacherService;
            _curriculumService = curriculumService;
            _departmentService = departmentService;
            _applicationService = applicationService;
            _publicAdmin = publicAdmin;
        }

        // ============== PUBLIC CONTENT (visible only) ==============
        [HttpGet("facilities")]
        public async Task<IActionResult> GetFacilities()
            => Ok(await _publicAdmin.GetFacilitiesAsync(onlyVisible: true));

        [HttpGet("graduates")]
        public async Task<IActionResult> GetGraduates()
            => Ok(await _publicAdmin.GetGraduatesAsync(onlyVisible: true));

        [HttpGet("faqs")]
        public async Task<IActionResult> GetFaqs()
            => Ok(await _publicAdmin.GetFaqsAsync(onlyVisible: true));

        // ============== PUBLIC APPLY (anonymous) ==============
        [HttpPost("applications")]
        public async Task<IActionResult> CreateApplication([FromBody] CreateCourseApplicationDto dto)
        {
            try
            {
                var id = await _applicationService.CreateAsync(dto);
                return Ok(new { id, message = "Müraciətiniz qəbul edildi. Komandamız sizinlə əlaqə saxlayacaq." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var courses = (await _courseService.GetAllAsync()).Count(c => c.IsActive);
            var teachers = (await _teacherService.GetAllTeachersAsync()).Count(t => t.IsActive);
            var groups = (await _groupService.GetAllAsync()).Count(g => g.IsActive);
            var departments = (await _departmentService.GetAllAsync()).Count(d => d.IsActive);
            return Ok(new PublicStatsDto
            {
                CourseCount = courses,
                TeacherCount = teachers,
                GroupCount = groups,
                DepartmentCount = departments
            });
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = (await _departmentService.GetAllAsync())
                .Where(d => d.IsActive)
                .Select(d => new PublicDepartmentDto { Id = d.Id, Name = d.Name });
            return Ok(departments);
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses()
        {
            var courses = (await _courseService.GetAllAsync()).Where(c => c.IsActive).ToList();
            var groups = (await _groupService.GetAllAsync()).ToList();
            var today = DateTime.UtcNow.Date;

            var result = new List<PublicCourseDto>();
            foreach (var c in courses)
            {
                var upcoming = groups.Count(g => g.CourseName == c.Name && g.IsActive && g.EndDate.Date >= today);
                int totalSessions = 0;
                try
                {
                    var tpl = await _curriculumService.GetActiveByCourseAsync(c.Id);
                    totalSessions = tpl?.ItemCount ?? 0;
                }
                catch { }

                result.Add(new PublicCourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    DepartmentName = c.DepartmentName,
                    ShortDescription = Truncate(c.Description, 140),
                    Level = c.Level,
                    Credit = c.Credit,
                    TotalHours = c.TotalHours,
                    TotalSessions = totalSessions,
                    UpcomingGroupCount = upcoming
                });
            }
            return Ok(result);
        }

        [HttpGet("courses/{id}")]
        public async Task<IActionResult> GetCourseDetail(int id)
        {
            var c = await _courseService.GetByIdAsync(id);
            if (c == null || !c.IsActive) return NotFound();

            // DepartmentName GetByIdAsync-də gəlmir → DepartmentId varsa adı həll edirik.
            var departmentName = c.DepartmentName;
            if (string.IsNullOrEmpty(departmentName) && c.DepartmentId.HasValue)
            {
                try { departmentName = (await _departmentService.GetByIdAsync(c.DepartmentId.Value))?.Name; }
                catch { }
            }

            var dto = new PublicCourseDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                DepartmentName = departmentName,
                Description = c.Description,
                Level = c.Level,
                Credit = c.Credit,
                TotalHours = c.TotalHours
            };

            // Curriculum overview + assessment plan (aktiv şablondan)
            try
            {
                var tpl = await _curriculumService.GetActiveByCourseAsync(id);
                if (tpl != null)
                {
                    var detail = await _curriculumService.GetDetailAsync(tpl.Id);
                    if (detail != null)
                    {
                        dto.CurriculumName = detail.Name;
                        dto.Curriculum = detail.Items
                            .OrderBy(i => i.SessionNumber)
                            .Select(i => new PublicCurriculumItemDto
                            {
                                SessionNumber = i.SessionNumber,
                                Topic = i.Topic,
                                HasAssessment = i.HasAssessment,
                                AssessmentTitle = i.AssessmentTitle
                            }).ToList();
                        dto.TotalSessions = detail.Items.Count;
                        dto.Assessments = detail.Items
                            .Where(i => i.HasAssessment)
                            .Select(i => new PublicAssessmentDto
                            {
                                Title = i.AssessmentTitle ?? i.Topic,
                                Type = i.AssessmentType?.ToString(),
                                WeightPercent = i.WeightPercent,
                                MaxScore = i.MaxScore
                            }).ToList();
                    }
                }
            }
            catch { }

            // Upcoming groups (həmin course üzrə aktiv qruplar)
            try
            {
                var today = DateTime.UtcNow.Date;
                var candidates = (await _groupService.GetAllAsync())
                    .Where(g => g.CourseName == c.Name && g.IsActive && g.EndDate.Date >= today)
                    .ToList();

                foreach (var g in candidates)
                {
                    var gd = await _groupService.GetByIdAsync(g.Id);
                    if (gd == null || gd.CourseId != id) continue;
                    dto.Groups.Add(new PublicGroupDto
                    {
                        Id = gd.Id,
                        Name = gd.Name,
                        SeasonName = gd.SeasonName,
                        TeacherName = gd.TeacherName,
                        StartDate = gd.StartDate,
                        EndDate = gd.EndDate,
                        AvailableSeats = Math.Max(0, gd.Capacity - gd.EnrolledCount),
                        ScheduleSummary = string.Join(", ", gd.ScheduleDays
                            .Select(s => $"{s.DayOfWeek} {s.StartTime}-{s.EndTime}"))
                    });
                }
                dto.Teachers = dto.Groups.Select(g => g.TeacherName).Distinct().ToList();
            }
            catch { }

            return Ok(dto);
        }

        [HttpGet("teachers")]
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = (await _teacherService.GetAllTeachersAsync())
                .Where(t => t.IsActive)
                .Select(t => new PublicTeacherDto
                {
                    Id = t.Id,
                    FullName = t.FullName,
                    Title = t.Title,
                    GroupCount = t.GroupCount
                });
            return Ok(teachers);
        }

        private static string? Truncate(string? text, int max)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            return text.Length <= max ? text : text.Substring(0, max).TrimEnd() + "…";
        }
    }
}
