using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface ICourseOfferingRepository : IGenericRepository<CourseOffering>
    {
        Task<CourseOffering?> GetOfferingWithDetailsAsync(int offeringId);
        Task<IEnumerable<CourseOffering>> GetActiveOfferingsAsync();
        Task<IEnumerable<CourseOffering>> GetOfferingsByTeacherAsync(int teacherId);
        Task<IEnumerable<CourseOffering>> GetOfferingsByTermAsync(int termId);
        Task<bool> HasCapacityAsync(int offeringId);
    }
}
