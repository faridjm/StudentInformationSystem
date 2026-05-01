using SIS.BLL.Services.DTOs.Grade;

namespace SIS.BLL.Services.Interfaces
{
    public interface IGradeService
    {
        Task<IEnumerable<GradeDto>> GetGradesByEnrollmentAsync(int enrollmentId);
        Task<IEnumerable<GradeDto>> GetGradesByStudentAsync(int studentId);
        Task AddGradeAsync(CreateGradeDto dto);
        Task UpdateGradeAsync(UpdateGradeDto dto);
        Task DeleteGradeAsync(int id);
    }
}
