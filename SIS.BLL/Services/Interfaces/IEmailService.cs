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

        Task SendQuizActivatedAsync(string to, string fullName, string quizTitle, string courseName, string groupName, string? deadline);
        Task SendAssignmentCreatedAsync(string to, string fullName, string assignmentTitle, string courseName, string groupName, DateTime dueDate, string? description);

        /// <summary>Transcript PDF-ni email attachment kimi göndərir.</summary>
        Task SendTranscriptAsync(string to, string fullName, string courseName, string groupName, byte[] pdfBytes, string fileName);
    }
}
