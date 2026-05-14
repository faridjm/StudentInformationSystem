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

        public AuthService(
            UserManager<AppUser> userManager,
            IOptions<JwtSettings> jwtSettings,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<TokenDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new BadRequestException("Email və ya şifrə yanlışdır");

            if (!user.IsActive)
                throw new UnauthorizedException("Hesabınız deaktiv edilmişdir");

            var isCorrect = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isCorrect)
                throw new BadRequestException("Email və ya şifrə yanlışdır");

            return await GenerateTokenAsync(user);
        }

        public async Task LogoutAsync(string userId)
        {
            await _unitOfWork.RefreshTokens.RevokeAllForUserAsync(userId);
            await _unitOfWork.SaveAsync();
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

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var frontendUrl = _configuration["App:FrontendUrl"];
            var resetLink = $"{frontendUrl}/Auth/ResetPassword?email={WebUtility.UrlEncode(email)}&token={WebUtility.UrlEncode(token)}";

            await _emailService.SendPasswordResetAsync(user.Email!, resetLink);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new BadRequestException("Şifrələr uyğun gəlmir");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new NotFoundException("İstifadəçi tapılmadı");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
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
