namespace SIS.BLL.Services.DTOs.Public
{
    /// <summary>Public (login tələb etməyən) marketing sayt üçün DTO-lar.
    /// Bütün data mövcud Course / Department / Group / CurriculumTemplate / Teacher
    /// servislərindən kompozisiya olunur. Heç bir fake data yoxdur.</summary>
    public class PublicCourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? DepartmentName { get; set; }
        public string? ShortDescription { get; set; }
        public string? Level { get; set; }
        public int Credit { get; set; }
        public int TotalHours { get; set; }
        public int TotalSessions { get; set; }
        public int UpcomingGroupCount { get; set; }
    }

    public class PublicGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? SeasonName { get; set; }
        public string TeacherName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ScheduleSummary { get; set; } = "";
        public int AvailableSeats { get; set; }
    }

    public class PublicCurriculumItemDto
    {
        public int SessionNumber { get; set; }
        public string Topic { get; set; } = null!;
        public bool HasAssessment { get; set; }
        public string? AssessmentTitle { get; set; }
    }

    public class PublicAssessmentDto
    {
        public string Title { get; set; } = null!;
        public string? Type { get; set; }
        public decimal? WeightPercent { get; set; }
        public decimal? MaxScore { get; set; }
    }

    public class PublicTeacherDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? Title { get; set; }
        public int GroupCount { get; set; }
    }

    public class PublicCourseDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? DepartmentName { get; set; }
        public string? Description { get; set; }
        public string? Level { get; set; }
        public int Credit { get; set; }
        public int TotalHours { get; set; }
        public int TotalSessions { get; set; }
        public string? CurriculumName { get; set; }
        public List<PublicCurriculumItemDto> Curriculum { get; set; } = new();
        public List<PublicAssessmentDto> Assessments { get; set; } = new();
        public List<PublicGroupDto> Groups { get; set; } = new();
        public List<string> Teachers { get; set; } = new();
    }

    public class PublicStatsDto
    {
        public int CourseCount { get; set; }
        public int TeacherCount { get; set; }
        public int GroupCount { get; set; }
        public int DepartmentCount { get; set; }
    }

    public class PublicDepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
