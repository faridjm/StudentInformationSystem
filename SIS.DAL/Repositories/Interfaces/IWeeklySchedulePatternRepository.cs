using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IWeeklySchedulePatternRepository : IGenericRepository<WeeklySchedulePattern>
    {
        Task<IEnumerable<WeeklySchedulePattern>> GetByCourseOfferingAsync(int courseOfferingId);
        Task DeleteByCourseOfferingAsync(int courseOfferingId);
    }
}
