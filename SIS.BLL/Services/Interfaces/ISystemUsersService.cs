using SIS.BLL.Services.DTOs.Common;
using SIS.BLL.Services.DTOs.SystemUsers;

namespace SIS.BLL.Services.Interfaces
{
    /// <summary>
    /// SuperAdmin tərəfindən idarə olunan sistem istifadəçiləri (Admin, Moderator, Teacher, Student, SuperAdmin).
    /// Mövcud StudentService/TeacherService domain məntiqini pozmur — yalnız identitet səviyyəsində CRUD edir.
    /// </summary>
    public interface ISystemUsersService
    {
        Task<PagedResult<SystemUserListItemDto>> GetPagedAsync(SystemUserFilterDto filter);
        Task<SystemUserDetailDto> GetByIdAsync(string userId);
        Task<string> CreateAsync(CreateSystemUserDto dto, string actingSuperAdminId);
        Task UpdateAsync(string userId, UpdateSystemUserDto dto, string actingSuperAdminId);
        Task ChangePasswordAsync(string userId, ChangeSystemUserPasswordDto dto, string actingSuperAdminId);
        Task SetStatusAsync(string userId, bool isActive, string actingSuperAdminId);
    }
}
