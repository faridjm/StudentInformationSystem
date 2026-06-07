using SIS.BLL.Services.DTOs.Common;
using SIS.BLL.Services.DTOs.Public;

namespace SIS.BLL.Services.Interfaces
{
    /// <summary>
    /// Public saytdan gələn kurs müraciətlərini idarə edən servis.
    /// Public submit anonim, admin əməliyyatları yalnız admin/superadmin üçündür.
    /// </summary>
    public interface ICourseApplicationService
    {
        /// <summary>Public form-dan müraciət yaradır. Email göndərmə fail olsa belə əməliyyat poz­ulmur.</summary>
        Task<int> CreateAsync(CreateCourseApplicationDto dto);

        Task<PagedResult<CourseApplicationListDto>> GetPagedAsync(CourseApplicationFilterDto filter);
        Task<CourseApplicationDetailDto?> GetDetailAsync(int id);

        Task UpdateStatusAsync(int id, UpdateApplicationStatusDto dto);
        Task UpdateAdminNoteAsync(int id, UpdateApplicationNoteDto dto);

        Task<ConvertApplicationResultDto> ConvertToStudentAsync(int id, ConvertApplicationToStudentDto dto);

        Task<CourseApplicationStatsDto> GetStatsAsync();
    }
}
