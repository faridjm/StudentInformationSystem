using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SIS.BLL.Services.DTOs.Auth;
using SIS.BLL.Services.Interfaces;
using SIS.Entities.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SIS.BLL.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<AppUser> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<TokenDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Email və ya şifrə yanlışdır");

            if (!user.IsActive)
                throw new Exception("Hesabınız deaktiv edilmişdir");

            var isCorrect = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isCorrect)
                throw new Exception("Email və ya şifrə yanlışdır");

            return await GenerateTokenAsync(user);
        }

        public async Task LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            await _userManager.RemoveAuthenticationTokenAsync(user, "SIS", "RefreshToken");
        }

        public async Task<TokenDto> RefreshTokenAsync(string refreshToken)
        {
            var users = _userManager.Users.ToList();
            AppUser? currentUser = null;

            foreach (var user in users)
            {
                var savedToken = await _userManager.GetAuthenticationTokenAsync(user, "SIS", "RefreshToken");
                if (savedToken == refreshToken)
                {
                    currentUser = user;
                    break;
                }
            }

            if (currentUser == null)
                throw new Exception("Yanlış refresh token");

            return await GenerateTokenAsync(currentUser);
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new Exception("Yeni şifrələr uyğun gəlmir");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("İstifadəçi tapılmadı");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return; 

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _userManager.SetAuthenticationTokenAsync(user, "SIS", "ResetToken", token);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new Exception("Şifrələr uyğun gəlmir");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("İstifadəçi tapılmadı");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
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
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:AccessTokenExpireMinutes"]!)),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = GenerateRefreshToken();
            await _userManager.SetAuthenticationTokenAsync(user, "SIS", "RefreshToken", refreshToken);

            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpireAt = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:AccessTokenExpireMinutes"]!))
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
