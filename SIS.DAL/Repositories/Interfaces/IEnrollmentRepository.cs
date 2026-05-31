using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IEnrollmentRepository : IGenericRepository<Enrollment>
    {
        Task<Enrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId);
        Task<IEnumerable<Enrollment>> GetByGroupAsync(int groupId);
        Task<IEnumerable<Enrollment>> GetByStudentAsync(int studentId);
        Task<bool> IsStudentEnrolledAsync(int studentId, int groupId);
    }
}
