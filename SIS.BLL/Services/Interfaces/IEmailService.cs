namespace SIS.BLL.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody);
        Task SendPasswordResetAsync(string to, string resetLink);
        Task SendWelcomeAsync(string to, string fullName, string password);
    }
}
