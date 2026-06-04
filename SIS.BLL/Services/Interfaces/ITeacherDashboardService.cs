using SIS.BLL.Services.DTOs.Dashboard;

namespace SIS.BLL.Services.Interfaces
{
    public interface ITeacherDashboardService
    {
        /// <summary>Cari müəllimin iş masası məlumatları (yalnız öz qrupları).</summary>
        Task<TeacherDashboardDto> GetDashboardAsync(string userId);
    }
}
