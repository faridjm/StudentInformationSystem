namespace SIS.BLL.Services.DTOs.Auth
{
    public class TokenDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpireAt { get; set; }
    }
}
