using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface ICourseApplicationRepository : IGenericRepository<CourseApplication>
    {
        /// <summary>Bütün əlaqələri (Course, Group, ConvertedStudent) ilə birlikdə gətirir.</summary>
        Task<CourseApplication?> GetWithRelationsAsync(int id);

        /// <summary>Filter + sıralama + paging tətbiq edir. UI list üçün.</summary>
        Task<(IReadOnlyList<CourseApplication> Items, int TotalCount)> QueryAsync(
            string? search,
            SIS.Entities.Enums.CourseApplicationStatus? status,
            int? courseId,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize);
    }
}
