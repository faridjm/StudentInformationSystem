namespace SIS.BLL.Services.DTOs.Auth
{
    public class SetPasswordDto
    {
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

    public class AdminSetPasswordDto
    {
        public string UserId { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }
}
