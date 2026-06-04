using Refit;
using SIS.BLL.Services.DTOs.Dashboard;

namespace SIS.UI.ApiClients
{
    public interface ITeacherDashboardApi
    {
        [Get("/api/teacherdashboard")]
        Task<TeacherDashboardDto> GetAsync();
    }
}
