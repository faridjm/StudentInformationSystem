using SIS.BLL.Services.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<TokenDto> LoginAsync(LoginDto dto);
        Task LogoutAsync(string userId);
        Task<TokenDto> RefreshTokenAsync(string refreshToken);
        Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task SetPasswordAsync(string userId, string newPassword);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto dto);
    }
}
