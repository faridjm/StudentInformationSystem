using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface ICurriculumTemplateRepository : IGenericRepository<CurriculumTemplate>
    {
        Task<IEnumerable<CurriculumTemplate>> GetByCourseAsync(int courseId);
        Task<CurriculumTemplate?> GetActiveByCourseAsync(int courseId);
        Task<CurriculumTemplate?> GetWithItemsAsync(int templateId);
    }
}
