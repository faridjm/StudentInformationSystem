using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IEnrollmentRepository : IGenericRepository<Enrollment>
    {
        Task<Enrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId);
        Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId);
        Task<IEnumerable<Enrollment>> GetCourseEnrollmentsAsync(int courseOfferingId);
        Task<bool> IsStudentEnrolledAsync(int studentId, int courseOfferingId);
    }
}
