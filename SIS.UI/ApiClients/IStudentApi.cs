using Refit;
using SIS.BLL.Services.DTOs.Student;

namespace SIS.UI.ApiClients
{
    public interface IStudentApi
    {
        [Get("/api/student")]
        Task<IEnumerable<StudentListDto>> GetAllAsync();

        [Get("/api/student/{id}")]
        Task<StudentDetailDto> GetByIdAsync(int id);

        [Get("/api/student/me")]
        Task<StudentDetailDto> GetMeAsync();

        [Post("/api/student")]
        Task CreateAsync([Body] CreateStudentDto dto);

        [Put("/api/student")]
        Task UpdateAsync([Body] UpdateStudentDto dto);

        [Delete("/api/student/{id}")]
        Task DeleteAsync(int id);
    }
}
