using SIS.BLL.Services.DTOs.Public;
using SIS.UI.ViewModels;

namespace SIS.UI.Services
{
    public interface IPublicWebsiteService
    {
        Task<PublicHomeViewModel> GetHomeAsync();
        Task<PublicCoursesViewModel> GetCoursesAsync(string? q, string? department, string? level);
        Task<PublicCourseDetailViewModel?> GetCourseDetailAsync(int id);
        Task<List<PublicTeacherDto>> GetTeachersAsync();
        Task<PublicAboutViewModel> GetAboutContentAsync();
        List<PublicGraduateViewModel> GetGraduates();
    }
}
