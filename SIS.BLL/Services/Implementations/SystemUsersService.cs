using Microsoft.AspNetCore.Identity;
using SIS.BLL.Services.DTOs.Common;
using SIS.BLL.Services.DTOs.SystemUsers;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using SIS.Entities.Identity;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    /// <summary>
    /// SuperAdmin-only sistem istifadəçi idarəetməsi.
    /// Audit log bütün vacib əməliyyatlara yazılır; parol heç vaxt loglanmır.
    /// </summary>
    public class SystemUsersService : ISystemUsersService
    {
        private static readonly string[] AllowedRoles = new[]
        {
            RoleConstants.SuperAdmin,
            RoleConstants.Admin,
            RoleConstants.Moderator,
            RoleConstants.Teacher,
            RoleConstants.Student
        };

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditLogService _audit;
        private readonly IAdminAccessService _adminAccess;
        private readonly IEmailService _email;

        public SystemUsersService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuditLogService audit,
            IAdminAccessService adminAccess,
            IEmailService email)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _audit = audit;
            _adminAccess = adminAccess;
            _email = email;
        }

        public async Task<PagedResult<SystemUserListItemDto>> GetPagedAsync(SystemUserFilterDto filter)
        {
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

            // EF Core-dan filter — UserManager-ın Users IQueryable-i istifadə olunur
            var q = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                q = q.Where(u =>
                    (u.FullName != null && u.FullName.Contains(s)) ||
                    (u.Email != null && u.Email.Contains(s)) ||
                    (u.UserName != null && u.UserName.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                if (string.Equals(filter.Status, "active", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(u => u.IsActive);
                else if (string.Equals(filter.Status, "disabled", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(u => !u.IsActive);
            }

            // Role filter — UserManager.GetUsersInRoleAsync çağırışı IDs ilə daralma üçün
            HashSet<string>? roleUserIds = null;
            if (!string.IsNullOrWhiteSpace(filter.Role) && AllowedRoles.Contains(filter.Role))
            {
                var inRole = await _userManager.GetUsersInRoleAsync(filter.Role!);
                roleUserIds = inRole.Select(u => u.Id).ToHashSet();
                q = q.Where(u => roleUserIds.Contains(u.Id));
            }

            var total = q.Count();

            var users = q
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = new List<SystemUserListItemDto>(users.Count);
            foreach (var u in users)
            {
                var role = await GetPrimaryRoleAsync(u);
                bool? adminAccessActive = null;
                if (role == RoleConstants.Admin)
                    adminAccessActive = await _adminAccess.HasActiveAdminAccessAsync(u.Id);

                items.Add(new SystemUserListItemDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role = role,
                    PanelAccess = DescribePanelAccess(role),
                    IsActive = u.IsActive,
                    EmailConfirmed = u.EmailConfirmed,
                    CreatedAt = u.CreatedAt,
                    AdminAccessActive = adminAccessActive
                });
            }

            return new PagedResult<SystemUserListItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<SystemUserDetailDto> GetByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı");

            var role = await GetPrimaryRoleAsync(user);
            bool? adminAccessActive = null;
            if (role == RoleConstants.Admin)
                adminAccessActive = await _adminAccess.HasActiveAdminAccessAsync(user.Id);

            return new SystemUserDetailDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = role,
                PanelAccess = DescribePanelAccess(role),
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                AdminAccessActive = adminAccessActive
            };
        }

        public async Task<string> CreateAsync(CreateSystemUserDto dto, string actingSuperAdminId)
        {
            ValidateCreateInput(dto);
            EnsureRoleAllowed(dto.Role);

            // Email unique
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                throw new BadRequestException("Bu email artıq mövcuddur");

            // Role mövcudluğu
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                throw new BadRequestException("Seçilmiş rol sistemdə tapılmadı");

            var normalizedPhone = await Helpers.PhoneUniqueValidator.ValidateAndNormalizeAsync(_userManager, dto.PhoneNumber);

            var user = new AppUser
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                UserName = dto.Email.Trim(),
                PhoneNumber = normalizedPhone,
                IsActive = dto.IsActive,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                throw new BadRequestException(string.Join(", ", createResult.Errors.Select(e => e.Description)));

            var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!roleResult.Succeeded)
            {
                // Rollback: yaradılmış istifadəçini sil ki, "asılı" qalmasın.
                await _userManager.DeleteAsync(user);
                throw new BadRequestException("Rol təyini alınmadı: " +
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            await _audit.LogAsync("CreateSystemUser", "AppUser", user.Id,
                $"SuperAdmin sistem istifadəçisi yaratdı: {user.FullName} ({user.Email}) — rol: {dto.Role}");

            // Welcome email (best-effort)
            if (dto.SendWelcomeEmail)
            {
                _ = SendWelcomeSafelyAsync(user.Email!, user.FullName, user.UserName!, dto.Password, dto.Role);
            }

            return user.Id;
        }

        public async Task UpdateAsync(string userId, UpdateSystemUserDto dto, string actingSuperAdminId)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new BadRequestException("Tam ad boş ola bilməz");
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new BadRequestException("Email boş ola bilməz");
            EnsureRoleAllowed(dto.Role);

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı");

            var oldRole = await GetPrimaryRoleAsync(user);
            var oldEmail = user.Email;
            var oldFullName = user.FullName;
            var oldIsActive = user.IsActive;

            // Self-protection: cari SuperAdmin öz hesabını dəyişərkən rolu və ya statusu aşağı sala bilməz
            if (string.Equals(user.Id, actingSuperAdminId, StringComparison.Ordinal))
            {
                if (oldRole == RoleConstants.SuperAdmin && dto.Role != RoleConstants.SuperAdmin)
                    throw new BadRequestException("SuperAdmin öz rolunu aşağı sala bilməz");
                if (oldIsActive && !dto.IsActive)
                    throw new BadRequestException("SuperAdmin öz hesabını deaktiv edə bilməz");
            }

            // Email unique check (özündən başqa kim istifadə edir?)
            if (!string.Equals(oldEmail, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var dup = await _userManager.FindByEmailAsync(dto.Email);
                if (dup != null && dup.Id != user.Id)
                    throw new BadRequestException("Bu email artıq mövcuddur");
            }

            user.FullName = dto.FullName.Trim();
            user.Email = dto.Email.Trim();
            user.UserName = dto.Email.Trim();
            user.NormalizedEmail = _userManager.NormalizeEmail(dto.Email);
            user.NormalizedUserName = _userManager.NormalizeName(dto.Email);
            user.PhoneNumber = await Helpers.PhoneUniqueValidator.ValidateAndNormalizeAsync(
                _userManager, dto.PhoneNumber, user.Id);
            user.IsActive = dto.IsActive;
            user.EmailConfirmed = dto.EmailConfirmed;
            user.UpdatedAt = DateTime.UtcNow;

            var updateRes = await _userManager.UpdateAsync(user);
            if (!updateRes.Succeeded)
                throw new BadRequestException(string.Join(", ", updateRes.Errors.Select(e => e.Description)));

            // Rol dəyişikliyi
            if (!string.Equals(oldRole, dto.Role, StringComparison.Ordinal))
            {
                if (!await _roleManager.RoleExistsAsync(dto.Role))
                    throw new BadRequestException("Seçilmiş rol sistemdə tapılmadı");

                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Count > 0)
                {
                    var rmRes = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!rmRes.Succeeded)
                        throw new BadRequestException("Köhnə rolların silinməsi alınmadı: " +
                            string.Join(", ", rmRes.Errors.Select(e => e.Description)));
                }
                var addRes = await _userManager.AddToRoleAsync(user, dto.Role);
                if (!addRes.Succeeded)
                    throw new BadRequestException("Yeni rol təyini alınmadı: " +
                        string.Join(", ", addRes.Errors.Select(e => e.Description)));

                await _audit.LogAsync("UpdateUserRole", "AppUser", user.Id,
                    $"İstifadəçi rolu dəyişdirildi: {user.FullName} ({user.Email}) — {oldRole} → {dto.Role}");
            }

            await _audit.LogAsync("UpdateSystemUser", "AppUser", user.Id,
                $"SuperAdmin istifadəçini yenilədi: {user.FullName} ({user.Email})");

            if (oldIsActive && !dto.IsActive)
                await _audit.LogAsync("DisableSystemUser", "AppUser", user.Id, $"İstifadəçi deaktiv edildi: {user.Email}");
            else if (!oldIsActive && dto.IsActive)
                await _audit.LogAsync("EnableSystemUser", "AppUser", user.Id, $"İstifadəçi aktivləşdirildi: {user.Email}");
        }

        public async Task ChangePasswordAsync(string userId, ChangeSystemUserPasswordDto dto, string actingSuperAdminId)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new BadRequestException("Yeni şifrə boş ola bilməz");
            if (!string.Equals(dto.NewPassword, dto.ConfirmPassword, StringComparison.Ordinal))
                throw new BadRequestException("Şifrələr uyğun gəlmir");

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı");

            // Reset token istifadə edərək təhlükəsiz şəkildə yeni şifrə təyin et
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetRes = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!resetRes.Succeeded)
                throw new BadRequestException(string.Join(", ", resetRes.Errors.Select(e => e.Description)));

            // ⚠️ Parol heç vaxt loglanmır — yalnız fakt qeyd olunur.
            await _audit.LogAsync("ChangeSystemUserPassword", "AppUser", user.Id,
                $"SuperAdmin istifadəçi şifrəsini dəyişdirdi: {user.Email}");
        }

        public async Task SetStatusAsync(string userId, bool isActive, string actingSuperAdminId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı");

            if (string.Equals(user.Id, actingSuperAdminId, StringComparison.Ordinal) && !isActive)
                throw new BadRequestException("SuperAdmin öz hesabını deaktiv edə bilməz");

            if (user.IsActive == isActive) return;

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded)
                throw new BadRequestException(string.Join(", ", res.Errors.Select(e => e.Description)));

            await _audit.LogAsync(isActive ? "EnableSystemUser" : "DisableSystemUser", "AppUser", user.Id,
                isActive
                    ? $"İstifadəçi aktivləşdirildi: {user.Email}"
                    : $"İstifadəçi deaktiv edildi: {user.Email}");
        }

        // ---------- helpers ----------

        private async Task<string> GetPrimaryRoleAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || roles.Count == 0) return "—";
            // Prioritet: SuperAdmin → Moderator → Admin → Teacher → Student → digər
            string[] priority = { RoleConstants.SuperAdmin, RoleConstants.Moderator, RoleConstants.Admin, RoleConstants.Teacher, RoleConstants.Student };
            foreach (var p in priority)
                if (roles.Contains(p)) return p;
            return roles.First();
        }

        private static string DescribePanelAccess(string role) => role switch
        {
            RoleConstants.SuperAdmin => "Academic Admin + PublicAdmin + AuditLogs",
            RoleConstants.Admin => "Academic Admin (yalnız aktiv access ilə)",
            RoleConstants.Moderator => "PublicAdmin",
            RoleConstants.Teacher => "Teacher Panel",
            RoleConstants.Student => "Student Panel",
            _ => "—"
        };

        private static void EnsureRoleAllowed(string role)
        {
            if (!AllowedRoles.Contains(role))
                throw new BadRequestException("Rol icazə verilmiş siyahıda deyil");
        }

        private static void ValidateCreateInput(CreateSystemUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new BadRequestException("Tam ad boş ola bilməz");
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new BadRequestException("Email boş ola bilməz");
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new BadRequestException("Şifrə boş ola bilməz");
            if (!string.Equals(dto.Password, dto.ConfirmPassword, StringComparison.Ordinal))
                throw new BadRequestException("Şifrələr uyğun gəlmir");
            if (dto.Password.Length < 8)
                throw new BadRequestException("Şifrə minimum 8 simvol olmalıdır");
        }

        private async Task SendWelcomeSafelyAsync(string email, string fullName, string userName, string password, string role)
        {
            try { await _email.SendWelcomeAsync(email, fullName, userName, password, role); }
            catch
            {
                try { await _audit.LogAsync("EmailSendFailed", "AppUser", null, $"Welcome email göndərmə uğursuz: {email}"); }
                catch { }
            }
        }
    }
}
