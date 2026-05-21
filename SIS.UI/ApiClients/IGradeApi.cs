using Refit;
using SIS.BLL.Services.DTOs.Grade;

namespace SIS.UI.ApiClients
{
    public interface IGradeApi
    {
        [Get("/api/grade/enrollment/{enrollmentId}")]
        Task<IEnumerable<GradeDto>> GetByEnrollmentAsync(int enrollmentId);

        [Get("/api/grade/student/{studentId}")]
        Task<IEnumerable<GradeDto>> GetByStudentAsync(int studentId);

        [Get("/api/grade/my-grades")]
        Task<IEnumerable<GradeDto>> GetMyGradesAsync();

        [Post("/api/grade")]
        Task AddAsync([Body] CreateGradeDto dto);

        [Put("/api/grade")]
        Task UpdateAsync([Body] UpdateGradeDto dto);

        [Delete("/api/grade/{id}")]
        Task DeleteAsync(int id);
    }


}
