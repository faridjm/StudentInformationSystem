using SIS.BLL.Services.DTOs.Public;
using SIS.UI.ApiClients;
using SIS.UI.ViewModels;

namespace SIS.UI.Services
{
    /// <summary>
    /// Public saytın data qatı: real akademik data API-dən (IPublicApi),
    /// marketing/mock məzmun isə PublicDemoContent-dən kompozisiya olunur.
    /// Controller heç bir sorğu yazmır — yalnız bu servisi çağırır.
    /// </summary>
    public class PublicWebsiteService : IPublicWebsiteService
    {
        private readonly IPublicApi _api;
        public PublicWebsiteService(IPublicApi api) { _api = api; }

        public async Task<PublicHomeViewModel> GetHomeAsync()
        {
            var vm = new PublicHomeViewModel
            {
                Facilities = PublicDemoContent.Facilities(),
                Graduates = PublicDemoContent.Graduates()
            };
            try { vm.Stats = await _api.GetStatsAsync(); } catch { vm.Stats = new PublicStatsDto(); }

            List<PublicCourseDto> courses = new();
            try { courses = (await _api.GetCoursesAsync()).ToList(); } catch { }
            vm.PopularCourses = courses
                .OrderByDescending(c => c.UpcomingGroupCount)
                .ThenBy(c => c.Name)
                .Take(6).ToList();

            try { vm.Teachers = (await _api.GetTeachersAsync()).Take(4).ToList(); } catch { }
            return vm;
        }

        public async Task<PublicCoursesViewModel> GetCoursesAsync(string? q, string? department, string? level)
        {
            var all = new List<PublicCourseDto>();
            try { all = (await _api.GetCoursesAsync()).ToList(); } catch { }

            var vm = new PublicCoursesViewModel
            {
                Q = q,
                SelectedDepartment = department,
                SelectedLevel = level,
                Departments = all.Where(c => !string.IsNullOrEmpty(c.DepartmentName))
                    .Select(c => c.DepartmentName!).Distinct().OrderBy(x => x).ToList(),
                Levels = all.Where(c => !string.IsNullOrEmpty(c.Level))
                    .Select(c => c.Level!).Distinct().OrderBy(x => x).ToList()
            };

            IEnumerable<PublicCourseDto> filtered = all;
            if (!string.IsNullOrWhiteSpace(q))
                filtered = filtered.Where(c => c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (c.ShortDescription ?? "").Contains(q, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(department))
                filtered = filtered.Where(c => c.DepartmentName == department);
            if (!string.IsNullOrWhiteSpace(level))
                filtered = filtered.Where(c => c.Level == level);

            vm.Courses = filtered.ToList();
            return vm;
        }

        public async Task<PublicCourseDetailViewModel?> GetCourseDetailAsync(int id)
        {
            PublicCourseDetailDto? course = null;
            try { course = await _api.GetCourseDetailAsync(id); } catch { }
            if (course == null) return null;
            return new PublicCourseDetailViewModel
            {
                Course = course,
                Facilities = PublicDemoContent.Facilities().Take(3).ToList()
            };
        }

        public async Task<List<PublicTeacherDto>> GetTeachersAsync()
        {
            try { return (await _api.GetTeachersAsync()).ToList(); }
            catch { return new List<PublicTeacherDto>(); }
        }

        public Task<PublicAboutViewModel> GetAboutContentAsync()
        {
            return Task.FromResult(new PublicAboutViewModel
            {
                Story = PublicDemoContent.Story,
                Mission = PublicDemoContent.Mission,
                Vision = PublicDemoContent.Vision,
                Facilities = PublicDemoContent.Facilities(),
                Timeline = PublicDemoContent.Timeline(),
                Methodology = PublicDemoContent.Methodology()
            });
        }

        public List<PublicGraduateViewModel> GetGraduates() => PublicDemoContent.Graduates();
    }
}
