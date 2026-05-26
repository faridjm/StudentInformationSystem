using SIS.BLL.Services.DTOs.ClassSession;

namespace SIS.BLL.Services.Interfaces
{
    public interface IClassSessionService
    {
        Task<ClassSessionDto?> GetByIdAsync(int id);
        Task<IEnumerable<ClassSessionDto>> GetByCourseOfferingAsync(int courseOfferingId);
        Task<IEnumerable<ClassSessionDto>> GetByCourseOfferingAndRangeAsync(
            int courseOfferingId, DateOnly from, DateOnly to);
        Task<IEnumerable<ClassSessionDto>> GetByStudentAndRangeAsync(
            int studentId, DateOnly from, DateOnly to);

        /// <summary>
        /// Həftəlik şablondan term aralığında olan bütün uyğun günlər üçün
        /// ClassSession-lar yaradır. Artıq mövcud sessiyalar dəyişdirilmir.
        /// </summary>
        Task<int> GenerateFromWeeklyScheduleAsync(int courseOfferingId);

        Task<int> CreateAsync(CreateClassSessionDto dto);
        Task UpdateAsync(UpdateClassSessionDto dto);
        Task CancelAsync(CancelClassSessionDto dto);
        Task<int> CreateMakeupAsync(CreateMakeupSessionDto dto);
        Task DeleteAsync(int id);
    }
}
