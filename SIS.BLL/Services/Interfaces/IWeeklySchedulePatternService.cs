using SIS.BLL.Services.DTOs.WeeklySchedule;

namespace SIS.BLL.Services.Interfaces
{
    public interface IWeeklySchedulePatternService
    {
        Task<IEnumerable<WeeklySchedulePatternDto>> GetByCourseOfferingAsync(int courseOfferingId);
        Task SetScheduleAsync(SetWeeklyScheduleDto dto);
        Task ClearScheduleAsync(int courseOfferingId);
    }
}
