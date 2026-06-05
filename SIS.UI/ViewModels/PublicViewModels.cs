using SIS.BLL.Services.DTOs.Public;

namespace SIS.UI.ViewModels
{
    // Public marketing sayt üçün kompozisiya viewmodel-ləri.
    // Real data BLL DTO-larından (PublicCourseDto və s.), marketing/mock data
    // isə PublicDemoContent-dən gəlir. İkisi aydın ayrılıb.

    public class PublicFacilityViewModel
    {
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public int Capacity { get; set; }
        public string Description { get; set; } = null!;
        public string Icon { get; set; } = "fa-door-open";
        public int Grad { get; set; }
    }

    public class PublicGraduateViewModel
    {
        public string FullName { get; set; } = null!;
        public string Course { get; set; } = null!;
        public int Year { get; set; }
        public string Position { get; set; } = null!;
        public string Story { get; set; } = null!;
        public string Initials => string.IsNullOrWhiteSpace(FullName) ? "?" :
            string.Concat(FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2).Select(p => p[0])).ToUpper();
        public int Grad { get; set; }
    }

    public class PublicTimelineItem
    {
        public string Year { get; set; } = null!;
        public string Text { get; set; } = null!;
    }

    public class PublicHomeViewModel
    {
        public PublicStatsDto Stats { get; set; } = new();
        public List<PublicCourseDto> PopularCourses { get; set; } = new();
        public List<PublicTeacherDto> Teachers { get; set; } = new();
        public List<PublicFacilityViewModel> Facilities { get; set; } = new();
        public List<PublicGraduateViewModel> Graduates { get; set; } = new();
    }

    public class PublicCoursesViewModel
    {
        public List<PublicCourseDto> Courses { get; set; } = new();
        public List<string> Departments { get; set; } = new();
        public List<string> Levels { get; set; } = new();
        public string? Q { get; set; }
        public string? SelectedDepartment { get; set; }
        public string? SelectedLevel { get; set; }
    }

    public class PublicCourseDetailViewModel
    {
        public PublicCourseDetailDto Course { get; set; } = new();
        public List<PublicFacilityViewModel> Facilities { get; set; } = new();
    }

    public class PublicAboutViewModel
    {
        public string Story { get; set; } = null!;
        public string Mission { get; set; } = null!;
        public string Vision { get; set; } = null!;
        public List<PublicFacilityViewModel> Facilities { get; set; } = new();
        public List<PublicTimelineItem> Timeline { get; set; } = new();
        public List<string> Methodology { get; set; } = new();
    }
}
