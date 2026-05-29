namespace SIS.BLL.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody);
        Task SendPasswordResetAsync(string to, string resetLink);
        Task SendWelcomeAsync(string to, string fullName, string userName, string password, string role);
        Task SendGradeNotificationAsync(string to, string fullName, string assignmentTitle, string courseName, decimal score, decimal maxScore, string? feedback);
        Task SendEnrollmentConfirmationAsync(string to, string fullName, string courseName, string teacherName, string termName);
        Task SendAnnouncementAsync(string to, string fullName, string title, string content);
    }
}
