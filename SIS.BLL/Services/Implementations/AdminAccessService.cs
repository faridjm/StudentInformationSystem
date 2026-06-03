using Microsoft.AspNetCore.Identity;
using SIS.BLL.Services.DTOs.AdminAccess;
using SIS.BLL.Services.Interfaces;
using SIS.Common.Constants;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Identity;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class AdminAccessService : IAdminAccessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAuditLogService _audit;
        private readonly INotificationService _notifications;

        public AdminAccessService(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, IAuditLogService audit, INotificationService notifications)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _audit = audit;
            _notifications = notifications;
        }

        public async Task<bool> HasActiveAdminAccessAsync(string adminUserId)
        {
            if (string.IsNullOrEmpty(adminUserId)) return false;

            var grant = await _unitOfWork.AdminAccessGrants.GetActiveGrantAsync(adminUserId);
            if (grant == null) return false;
            if (grant.ExpiresAt.HasValue && grant.ExpiresAt.Value <= DateTime.UtcNow) return false;
            return true;
        }

        public async Task ActivateAsync(string adminUserId, DateTime? expiresAt, string? reason, string grantedBySuperAdminId)
        {
            if (string.IsNullOrEmpty(adminUserId))
                throw new BadRequestException("İstifadəçi seçilməyib");

            if (adminUserId == grantedBySuperAdminId)
                throw new BadRequestException("Admin öz access-ini aktiv edə bilməz");

            var user = await _userManager.FindByIdAsync(adminUserId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı");

            if (!await _userManager.IsInRoleAsync(user, RoleConstants.Admin))
                throw new BadRequestException("Access yalnız Admin roluna malik istifadəçiyə verilə bilər");

            if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
                throw new BadRequestException("Bitmə tarixi gələcəkdə olmalıdır");

            // Mövcud aktiv grant-ları əvəz et
            var existing = await _unitOfWork.AdminAccessGrants.GetActiveGrantsAsync(adminUserId);
            foreach (var g in existing)
            {
                g.IsActive = false;
                g.RevokedAt = DateTime.UtcNow;
                g.RevokedBySuperAdminId = grantedBySuperAdminId;
                _unitOfWork.AdminAccessGrants.Update(g);
            }

            var grant = new AdminAccessGrant
            {
                AdminUserId = adminUserId,
                GrantedBySuperAdminId = grantedBySuperAdminId,
                GrantedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsActive = true,
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AdminAccessGrants.AddAsync(grant);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Activate", "AdminAccess", adminUserId,
                $"Admin panel girişi aktivləşdirildi: {user.FullName}" +
                (expiresAt.HasValue ? $" (bitmə: {expiresAt.Value:dd.MM.yyyy HH:mm})" : " (limitsiz)") +
                (string.IsNullOrWhiteSpace(reason) ? "" : $" — səbəb: {reason}"));

            await _notifications.SendNotificationAsync(adminUserId,
                "Admin girişi aktivləşdirildi",
                expiresAt.HasValue ? $"Admin panelə girişiniz {expiresAt.Value:dd.MM.yyyy HH:mm}-dək aktivdir" : "Admin panelə girişiniz aktivdir",
                NotificationType.AdminAccessChanged, "/Admin/Dashboard");
        }

        public async Task DeactivateAsync(string adminUserId, string? reason, string revokedBySuperAdminId)
        {
            if (string.IsNullOrEmpty(adminUserId))
                throw new BadRequestException("İstifadəçi seçilməyib");

            var user = await _userManager.FindByIdAsync(adminUserId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı");

            var existing = (await _unitOfWork.AdminAccessGrants.GetActiveGrantsAsync(adminUserId)).ToList();
            if (existing.Count == 0)
                throw new BadRequestException("Bu istifadəçinin aktiv girişi yoxdur");

            foreach (var g in existing)
            {
                g.IsActive = false;
                g.RevokedAt = DateTime.UtcNow;
                g.RevokedBySuperAdminId = revokedBySuperAdminId;
                if (!string.IsNullOrWhiteSpace(reason))
                    g.Reason = string.IsNullOrWhiteSpace(g.Reason) ? reason : $"{g.Reason} | Ləğv: {reason}";
                _unitOfWork.AdminAccessGrants.Update(g);
            }
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Deactivate", "AdminAccess", adminUserId,
                $"Admin panel girişi dayandırıldı: {user.FullName}" +
                (string.IsNullOrWhiteSpace(reason) ? "" : $" — səbəb: {reason}"));

            await _notifications.SendNotificationAsync(adminUserId,
                "Admin girişi dayandırıldı",
                "Admin panelə girişiniz SuperAdmin tərəfindən dayandırıldı",
                NotificationType.AdminAccessChanged, null);
        }

        public async Task<IEnumerable<AdminAccessUserDto>> GetAdminAccessUsersAsync()
        {
            var admins = await _userManager.GetUsersInRoleAsync(RoleConstants.Admin);
            var result = new List<AdminAccessUserDto>();

            foreach (var u in admins)
            {
                var grant = await _unitOfWork.AdminAccessGrants.GetLatestGrantAsync(u.Id);
                string status = "Inactive";
                if (grant != null && grant.IsActive && grant.RevokedAt == null)
                    status = (grant.ExpiresAt.HasValue && grant.ExpiresAt.Value <= DateTime.UtcNow) ? "Expired" : "Active";

                result.Add(new AdminAccessUserDto
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Status = status,
                    GrantedAt = grant?.GrantedAt,
                    ExpiresAt = grant?.ExpiresAt,
                    RevokedAt = grant?.RevokedAt,
                    Reason = grant?.Reason
                });
            }

            return result.OrderBy(r => r.FullName);
        }
    }
}
