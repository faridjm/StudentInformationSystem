using SIS.DAL.Context;
using SIS.DAL.Repositories.Interfaces;
using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Implementations
{
    public class CurriculumTemplateItemRepository : GenericRepository<CurriculumTemplateItem>, ICurriculumTemplateItemRepository
    {
        public CurriculumTemplateItemRepository(AppDbContext context) : base(context) { }
    }
}
