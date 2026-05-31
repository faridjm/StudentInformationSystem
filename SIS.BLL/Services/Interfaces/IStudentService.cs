using SIS.BLL.Services.DTOs.Student;

namespace SIS.BLL.Services.Interfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentListDto>> GetAllStudentsAsync();
        Task<StudentDetailDto?> GetStudentByIdAsync(int id);
        Task<StudentDetailDto?> GetStudentByUserIdAsync(string userId);
        Task CreateStudentAsync(CreateStudentDto dto);
        Task UpdateStudentAsync(UpdateStudentDto dto);
        Task DeleteStudentAsync(int id);
        Task UpdateGPAAsync(int studentId);
    }
}
