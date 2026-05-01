using SIS.BLL.Services.DTOs.CourseOffering;

namespace SIS.BLL.Services.Interfaces
{
    public interface ICourseOfferingService
    {
        Task<IEnumerable<CourseOfferingListDto>> GetAllOfferingsAsync();
        Task<CourseOfferingDetailDto?> GetOfferingByIdAsync(int id);
        Task<IEnumerable<CourseOfferingListDto>> GetActiveOfferingsAsync();
        Task<IEnumerable<CourseOfferingListDto>> GetOfferingsByTeacherAsync(int teacherId);
        Task<IEnumerable<CourseOfferingListDto>> GetOfferingsByTermAsync(int termId);
        Task CreateOfferingAsync(CreateCourseOfferingDto dto);
        Task UpdateOfferingAsync(UpdateCourseOfferingDto dto);
        Task DeleteOfferingAsync(int id);
    }
}
