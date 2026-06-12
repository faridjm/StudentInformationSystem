using SIS.Entities.Models;

namespace SIS.DAL.Repositories.Interfaces
{
    public interface IParentStudentRepository : IGenericRepository<ParentStudent>
    {
        /// <summary>Bir valideynin bütün əlaqələri (tələbə + AppUser ilə).</summary>
        Task<List<ParentStudent>> GetByParentAsync(string parentUserId, bool onlyActive = false);

        /// <summary>Bir tələbənin bütün valideyn əlaqələri (valideyn AppUser ilə).</summary>
        Task<List<ParentStudent>> GetByStudentAsync(int studentId);

        /// <summary>Konkret valideyn–tələbə əlaqəsi.</summary>
        Task<ParentStudent?> GetRelationAsync(string parentUserId, int studentId);

        /// <summary>Valideynin həmin tələbəyə aktiv (IsActive) çıxışı varmı?</summary>
        Task<bool> HasActiveAccessAsync(string parentUserId, int studentId);
    }
}
