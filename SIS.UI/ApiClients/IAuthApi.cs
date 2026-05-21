using Refit;
using SIS.BLL.Services.DTOs;
using SIS.BLL.Services.DTOs.Auth;

namespace SIS.UI.ApiClients
{
    public interface IAuthApi
    {
        [Post("/api/auth/login")]
        Task<TokenDto> LoginAsync([Body] LoginDto dto);

        [Post("/api/auth/refresh-token")]
        Task<TokenDto> RefreshTokenAsync([Body] string refreshToken);

        [Post("/api/auth/logout")]
        Task LogoutAsync();

        [Post("/api/auth/forgot-password")]
        Task ForgotPasswordAsync([Body] string email);

        [Post("/api/auth/reset-password")]
        Task ResetPasswordAsync([Body] ResetPasswordDto dto);

        [Post("/api/auth/change-password")]
        Task ChangePasswordAsync([Body] ChangePasswordDto dto);
    }

}
