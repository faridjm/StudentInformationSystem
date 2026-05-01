using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IAttendanceRecordRepository : IGenericRepository<AttendanceRecord>
    {
        Task<IEnumerable<AttendanceRecord>> GetAttendanceByEnrollmentAsync(int enrollmentId);
        Task<IEnumerable<AttendanceRecord>> GetAttendanceByDateAsync(int courseOfferingId, DateTime date);
        Task<double> GetAttendancePercentageAsync(int enrollmentId);
    }
}
