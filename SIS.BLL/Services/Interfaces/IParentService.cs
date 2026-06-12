using SIS.BLL.Services.DTOs.Parent;

namespace SIS.BLL.Services.Interfaces
{
    /// <summary>
    /// Valideyn–tələbə əlaqələrinin idarəsi (admin) + valideyn çıxış yoxlaması (Phase 2 panel üçün).
    /// Bütün read əməliyyatlarında parent-student access yoxlanmalıdır.
    /// </summary>
    public interface IParentService
    {
        // ── Admin ──
        Task<ParentCreatedResultDto> CreateParentForStudentAsync(int studentId, CreateParentForStudentDto dto);
        Task LinkExistingParentAsync(int studentId, LinkExistingParentDto dto);
        Task SetRelationActiveAsync(int studentId, string parentUserId, bool isActive);
        Task<List<ParentInfoDto>> GetParentsForStudentAsync(int studentId);

        // ── Parent panel (read-only) ──
        /// <summary>Valideynin aktiv əlaqəli (IsActive) övladları.</summary>
        Task<List<ParentChildDto>> GetChildrenAsync(string parentUserId);

        /// <summary>Valideynin həmin tələbəyə aktiv çıxışı yoxdursa UnauthorizedAccessException atır.</summary>
        Task EnsureParentCanAccessAsync(string parentUserId, int studentId);
    }
}
