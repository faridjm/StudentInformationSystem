using SIS.BLL.Services.DTOs.Student;
using SIS.Common.Pagination;

namespace SIS.BLL.Services.Interfaces
{
    public interface IStudentService
    {
        Task<PagedResult<StudentListDto>> GetAllStudentsAsync(PagedRequest request);
        Task<StudentDetailDto?> GetStudentByIdAsync(int id);
        Task<StudentDetailDto?> GetStudentByUserIdAsync(string userId);
        Task<IEnumerable<StudentListDto>> GetStudentsByProgramAsync(int programId);
        Task CreateStudentAsync(CreateStudentDto dto);
        Task UpdateStudentAsync(UpdateStudentDto dto);
        Task DeleteStudentAsync(int id);
        Task UpdateGPAAsync(int studentId);
        
    }
}
