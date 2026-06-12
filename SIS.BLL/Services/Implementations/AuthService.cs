using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SIS.BLL.Configuration;
using SIS.BLL.Services.DTOs.Auth;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Identity;
using SIS.Entities.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IAuditLogService _audit;

        public AuthService(
            UserManager<AppUser> userManager,
            IOptions<JwtSettings> jwtSettings,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IConfiguration configuration,
            IAuditLogService audit)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuration = configuration;
            _audit = audit;
        }

        public async Task<TokenDto> LoginAsync(LoginDto dto)
        {
            AppUser? user = null;

            // Əvvəlcə email ilə axtar
            user = await _userManager.FindByEmailAsync(dto.Email);

            // Tapılmadısa StudentNumber ilə axtar
            if (user == null)
            {
                var student = await _unitOfWork.Students
                    .GetAsync(s => s.StudentNumber == dto.Email);
                if (student != null)
                    user = await _userManager.FindByIdAsync(student.AppUserId);
            }

            // Tapılmadısa EmployeeNumber ilə axtar
            if (user == null)
            {
                var teacher = await _unitOfWork.Teachers
                    .GetAsync(t => t.EmployeeNumber == dto.Email);
                if (teacher != null)
                    user = await _userManager.FindByIdAsync(teacher.AppUserId);
            }

            if (user == null)
                throw new BadRequestException("Email/kod və ya şifrə yanlışdır");

            if (!user.IsActive)
                throw new UnauthorizedException("Hesabınız deaktiv edilmişdir");

            var isCorrect = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isCorrect)
            {
                _ = SafeAuditAsync("LoginFailed", "Auth", null, $"Yanlış şifrə cəhdi: {dto.Email}");
                throw new BadRequestException("Email/kod və ya şifrə yanlışdır");
            }

            var token = await GenerateTokenAsync(user);

            // Uğurlu login auditə düşür (HTTP context hələ anonymous, ona görə UserId description-a yazılır)
            var roles = await _userManager.GetRolesAsync(user);
            _ = SafeAuditAsync("LoginSuccess", "Auth", user.Id,
                $"İstifadəçi sistemə daxil oldu: {user.FullName} ({user.Email}) — {string.Join(",", roles)}");

            return token;
        }

        private async Task SafeAuditAsync(string action, string entity, string? entityId, string description)
        {
            try { await _audit.LogAsync(action, entity, entityId, description); }
            catch { /* heç vaxt login axınını sındırma */ }
        }

        public async Task LogoutAsync(string userId)
        {
            await _unitOfWork.RefreshTokens.RevokeAllForUserAsync(userId);
            await _unitOfWork.SaveAsync();
            _ = SafeAuditAsync("Logout", "Auth", userId, "İstifadəçi sistemdən çıxdı");
        }

        public async Task<TokenDto> RefreshTokenAsync(string refreshToken)
        {
            var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

            if (token == null)
                throw new UnauthorizedException("Yanlış refresh token");

            if (token.IsRevoked)
                throw new UnauthorizedException("Refresh token ləğv edilmişdir");

            if (token.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token müddəti bitmişdir");

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(token);

            var newTokenDto = await GenerateTokenAsync(token.User);

            token.ReplacedByToken = newTokenDto.RefreshToken;
            await _unitOfWork.SaveAsync();

            return newTokenDto;
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new BadRequestException("Şifrələr uyğun gəlmir");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("İstifadəçi tapılmadı");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task SetPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("İstifadəçi tapılmadı");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var frontendUrl = _configuration["App:FrontendUrl"];
            var safeToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token))
                .Replace('+', '-').Replace('/', '_').Replace("=", "");
            var resetLink = $"{frontendUrl}/Auth/ResetPassword?email={WebUtility.UrlEncode(email)}&token={safeToken}";

            await _emailService.SendPasswordResetAsync(user.Email!, resetLink);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new BadRequestException("Şifrələr uyğun gəlmir");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new NotFoundException("İstifadəçi tapılmadı");

            // Base64Url decode
            var base64 = dto.Token.Replace('-', '+').Replace('_', '/');
            var padding = base64.Length % 4 == 0 ? "" : new string('=', 4 - base64.Length % 4);
            var rawToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64 + padding));

            var result = await _userManager.ResetPasswordAsync(user, rawToken, dto.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        private async Task<TokenDto> GenerateTokenAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.FullName),
        };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpireMinutes),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshTokenValue = GenerateRefreshToken();
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenValue,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays),
                IsRevoked = false
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveAsync();

            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                ExpireAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpireMinutes)
            };
        }

        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
