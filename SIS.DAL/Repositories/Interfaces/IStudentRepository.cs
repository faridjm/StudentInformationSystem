using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        Task<Student?> GetStudentWithDetailsAsync(int studentId);
        Task<Student?> GetStudentByUserIdAsync(string userId);
        Task<IEnumerable<Student>> GetAllWithUsersAsync();
        /// <summary>Bütün student-lərin (silinmiş daxil) StudentNumber-lərini qaytarır.</summary>
        Task<IEnumerable<string?>> GetAllStudentNumbersAsync();
    }
}
