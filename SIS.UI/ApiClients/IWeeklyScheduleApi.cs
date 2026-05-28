using Refit;
using SIS.BLL.Services.DTOs.WeeklySchedule;

namespace SIS.UI.ApiClients
{
    public interface IWeeklyScheduleApi
    {
        [Get("/api/weeklyschedule/course/{courseOfferingId}")]
        Task<IEnumerable<WeeklySchedulePatternDto>> GetByCourseAsync(int courseOfferingId);

        [Post("/api/weeklyschedule")]
        Task SetAsync([Body] SetWeeklyScheduleDto dto);

        [Delete("/api/weeklyschedule/course/{courseOfferingId}")]
        Task ClearAsync(int courseOfferingId);
    }
}
