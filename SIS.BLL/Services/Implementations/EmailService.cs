using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SIS.BLL.Services.Interfaces;

namespace SIS.BLL.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var smtpHost = _configuration["Email:SmtpHost"]!;
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]!);
            var from = _configuration["Email:From"]!;
            var username = _configuration["Email:Username"]!;
            var password = _configuration["Email:Password"]!;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Acadex", from));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendPasswordResetAsync(string to, string resetLink)
        {
            var htmlBody = $@"
        <!DOCTYPE html><html>
        <body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
            <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:10px;padding:30px;'>
                <h2 style='color:#e94560;'>Şifrə Sıfırlama</h2>
                <p>Şifrənizi sıfırlamaq üçün aşağıdakı düyməyə basın:</p>
                <a href='{resetLink}' style='display:inline-block;background:#e94560;color:#fff;
                   padding:12px 24px;border-radius:8px;text-decoration:none;font-weight:bold;margin:20px 0;'>
                    Şifrəni Sıfırla
                </a>
                <p style='color:#666;font-size:13px;'>Bu link 24 saat ərzində etibarlıdır.</p>
                <hr style='border:none;border-top:1px solid #eee;'>
                <p style='color:#999;font-size:12px;'>Acadex</p>
            </div>
        </body></html>";

            await SendAsync(to, "Şifrə Sıfırlama — Acadex", htmlBody);
        }

        public async Task SendWelcomeAsync(string to, string fullName, string userName, string password, string role)
        {
            var roleAz = role == "Teacher" ? "Müəllim" : "Tələbə";

            var htmlBody = $@"
        <!DOCTYPE html><html>
        <body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
            <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:10px;padding:30px;'>
                <h2 style='color:#e94560;'>Xoş Gəldiniz, {fullName}!</h2>
                <p>Acadex sisteminə <strong>{roleAz}</strong> kimi qeydiyyatınız tamamlandı.</p>
                <div style='background:#f8f8f8;border-radius:8px;padding:16px;margin:20px 0;'>
                    <p style='margin:0;'><strong>İstifadəçi adı:</strong>
                        <code style='background:#eee;padding:2px 6px;border-radius:4px;'>{userName}</code></p>
                    <p style='margin:8px 0 0;'><strong>Şifrə:</strong>
                        <code style='background:#eee;padding:2px 6px;border-radius:4px;'>{password}</code></p>
                </div>
                <p>Sistemə daxil olmaq üçün bu məlumatlardan istifadə edə bilərsiniz.<br>
                   İstəsəniz daxil olduqdan sonra yeni şifrə təyin edə bilərsiniz.</p>
                <p style='color:#e94560;font-size:13px;'>⚠️ Şifrənizi heç kimlə paylaşmayın.</p>
                <hr style='border:none;border-top:1px solid #eee;'>
                <p style='color:#999;font-size:12px;'>Acadex</p>
            </div>
        </body></html>";

            await SendAsync(to, "Xoş Gəldiniz — Acadex", htmlBody);
        }

        public async Task SendGradeNotificationAsync(string to, string fullName, string assignmentTitle, string courseName, decimal score, decimal maxScore, string? feedback)
        {
            var percent = maxScore > 0 ? Math.Round(score / maxScore * 100, 1) : 0;
            var htmlBody = $@"
        <!DOCTYPE html><html>
        <body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
            <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:10px;padding:30px;'>
                <h2 style='color:#e94560;'>Tapşırıq Qiymətləndirildi</h2>
                <p>Hörmətli <strong>{fullName}</strong>,</p>
                <p><strong>{courseName}</strong> kursu üzrə <strong>{assignmentTitle}</strong> tapşırığınız qiymətləndirildi.</p>
                <div style='background:#f8f8f8;border-radius:8px;padding:16px;margin:20px 0;'>
                    <p style='margin:0;font-size:24px;font-weight:bold;color:#e94560;'>{score} / {maxScore} <span style='font-size:14px;color:#666;'>({percent}%)</span></p>
                    {(string.IsNullOrEmpty(feedback) ? "" : $"<p style='margin:8px 0 0;color:#444;'><strong>Rəy:</strong> {feedback}</p>")}
                </div>
                <p style='color:#999;font-size:12px;'>Acadex</p>
            </div>
        </body></html>";

            await SendAsync(to, $"Qiymət: {assignmentTitle} — Acadex", htmlBody);
        }

        public async Task SendEnrollmentConfirmationAsync(string to, string fullName, string courseName, string teacherName, string termName)
        {
            var htmlBody = $@"
        <!DOCTYPE html><html>
        <body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
            <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:10px;padding:30px;'>
                <h2 style='color:#e94560;'>Kurs Qeydiyyatı Tamamlandı</h2>
                <p>Hörmətli <strong>{fullName}</strong>,</p>
                <p>Aşağıdakı kursa qeydiyyatınız uğurla tamamlandı:</p>
                <div style='background:#f8f8f8;border-radius:8px;padding:16px;margin:20px 0;'>
                    <p style='margin:0;'><strong>Kurs:</strong> {courseName}</p>
                    <p style='margin:8px 0 0;'><strong>Müəllim:</strong> {teacherName}</p>
                    <p style='margin:8px 0 0;'><strong>Semestr:</strong> {termName}</p>
                </div>
                <p style='color:#999;font-size:12px;'>Acadex</p>
            </div>
        </body></html>";

            await SendAsync(to, $"Qeydiyyat: {courseName} — Acadex", htmlBody);
        }

        public async Task SendAnnouncementAsync(string to, string fullName, string title, string content)
        {
            var htmlBody = $@"
        <!DOCTYPE html><html>
        <body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
            <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:10px;padding:30px;'>
                <h2 style='color:#e94560;'>📢 {title}</h2>
                <p>Hörmətli <strong>{fullName}</strong>,</p>
                <div style='background:#f8f8f8;border-radius:8px;padding:16px;margin:20px 0;line-height:1.6;'>
                    {content}
                </div>
                <p style='color:#999;font-size:12px;'>Acadex</p>
            </div>
        </body></html>";

            await SendAsync(to, $"Elan: {title} — Acadex", htmlBody);
        }

        public async Task SendQuizActivatedAsync(string to, string fullName, string quizTitle, string courseName, string groupName, string? deadline)
        {
            var deadlineLine = string.IsNullOrEmpty(deadline) ? "" : $"<p style='margin:8px 0 0;'><strong>Son tarix:</strong> {deadline}</p>";
            var htmlBody = $@"
        <!DOCTYPE html><html>
        <body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
            <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:10px;padding:30px;'>
                <h2 style='color:#e94560;'>📝 Yeni Quiz Aktiv Edildi</h2>
                <p>Hörmətli <strong>{fullName}</strong>,</p>
                <p><strong>{courseName}</strong> kursu üzrə yeni quiz aktivləşdirildi:</p>
                <div style='background:#f8f8f8;border-radius:8px;padding:16px;margin:20px 0;'>
                    <p style='margin:0;font-size:16px;font-weight:bold;'>{quizTitle}</p>
                    <p style='margin:8px 0 0;'><strong>Qrup:</strong> {groupName}</p>
                    {deadlineLine}
                </div>
                <p>Quiz-ə cavab vermək üçün tələbə panelinizə daxil olun.</p>
                <p style='color:#999;font-size:12px;'>Acadex</p>
            </div>
        </body></html>";

            await SendAsync(to, $"Quiz Aktiv: {quizTitle} — Acadex", htmlBody);
        }

        public async Task SendAssignmentCreatedAsync(string to, string fullName, string assignmentTitle, string courseName, string groupName, DateTime dueDate, string? description)
        {
            var descBlock = string.IsNullOrEmpty(description)
                ? ""
                : $"<p style='margin:8px 0 0;color:#555;'><strong>Təsvir:</strong> {System.Net.WebUtility.HtmlEncode(description.Length > 200 ? description.Substring(0, 200) + "..." : description)}</p>";
            var htmlBody = $@"
        <!DOCTYPE html><html>
        <body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
            <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:10px;padding:30px;'>
                <h2 style='color:#e94560;'>📋 Yeni Tapşırıq</h2>
                <p>Hörmətli <strong>{fullName}</strong>,</p>
                <p><strong>{courseName}</strong> kursu üzrə yeni tapşırıq əlavə edildi:</p>
                <div style='background:#f8f8f8;border-radius:8px;padding:16px;margin:20px 0;'>
                    <p style='margin:0;font-size:16px;font-weight:bold;'>{assignmentTitle}</p>
                    <p style='margin:8px 0 0;'><strong>Qrup:</strong> {groupName}</p>
                    <p style='margin:8px 0 0;'><strong>Son tarix:</strong> {dueDate:dd.MM.yyyy HH:mm}</p>
                    {descBlock}
                </div>
                <p>Tapşırığa baxmaq və cavab vermək üçün tələbə panelinizə daxil olun.</p>
                <p style='color:#999;font-size:12px;'>Acadex</p>
            </div>
        </body></html>";

            await SendAsync(to, $"Yeni Tapşırıq: {assignmentTitle} — Acadex", htmlBody);
        }

        public async Task SendTranscriptAsync(string to, string fullName, string courseName, string groupName, byte[] pdfBytes, string fileName, string lang = "az")
        {
            lang = (lang ?? "az").Trim().ToLowerInvariant();
            if (lang != "en" && lang != "ru") lang = "az";

            // Dilə uyğun subject / başlıq / mətn (database data — ad, kurs, qrup — toxunulmur)
            string subject, heading, greeting, intro, courseLabel, groupLabel, fileLabel;
            switch (lang)
            {
                case "en":
                    subject = "Your Acadex 360 Academic Transcript";
                    heading = "Academic Transcript";
                    greeting = $"Hello <strong>{fullName}</strong>,";
                    intro = $"Your academic transcript for the <strong>{courseName}</strong> course has been attached to this email.";
                    courseLabel = "Course"; groupLabel = "Group"; fileLabel = "File";
                    break;
                case "ru":
                    subject = "Ваша академическая выписка Acadex 360";
                    heading = "Академическая выписка";
                    greeting = $"Здравствуйте, <strong>{fullName}</strong>,";
                    intro = $"Ваша академическая выписка по курсу <strong>{courseName}</strong> прикреплена к этому письму.";
                    courseLabel = "Курс"; groupLabel = "Группа"; fileLabel = "Файл";
                    break;
                default:
                    subject = "Acadex 360 Akademik çıxarışınız";
                    heading = "Akademik çıxarış";
                    greeting = $"Salam <strong>{fullName}</strong>,";
                    intro = $"Sizin <strong>{courseName}</strong> kursu üzrə akademik çıxarışınız bu emailə əlavə olunub.";
                    courseLabel = "Kurs"; groupLabel = "Qrup"; fileLabel = "Fayl";
                    break;
            }

            var htmlBody = $@"
        <!DOCTYPE html><html>
        <body style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
            <div style='max-width:600px;margin:0 auto;background:#fff;border-radius:10px;padding:30px;'>
                <h2 style='color:#4F46E5;'>📄 {heading} <span style='font-size:13px;color:#10B981;'>Acadex 360</span></h2>
                <p>{greeting}</p>
                <p>{intro}</p>
                <div style='background:#f8f8f8;border-radius:8px;padding:16px;margin:20px 0;'>
                    <p style='margin:0;'><strong>{courseLabel}:</strong> {courseName}</p>
                    <p style='margin:8px 0 0;'><strong>{groupLabel}:</strong> {groupName}</p>
                    <p style='margin:8px 0 0;'><strong>{fileLabel}:</strong> {fileName}</p>
                </div>
                <p style='color:#999;font-size:12px;'>Acadex 360 · Digital Academy Management Platform</p>
            </div>
        </body></html>";

            await SendWithAttachmentAsync(to, subject, htmlBody, pdfBytes, fileName, "application/pdf");
        }

        private async Task SendWithAttachmentAsync(string to, string subject, string htmlBody, byte[] attachment, string fileName, string contentType)
        {
            var smtpHost = _configuration["Email:SmtpHost"]!;
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]!);
            var from = _configuration["Email:From"]!;
            var username = _configuration["Email:Username"]!;
            var password = _configuration["Email:Password"]!;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Acadex", from));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            var ctParts = contentType.Split('/', 2);
            var mediaType = ctParts.Length > 0 ? ctParts[0] : "application";
            var mediaSubtype = ctParts.Length > 1 ? ctParts[1] : "octet-stream";
            builder.Attachments.Add(fileName, attachment, new ContentType(mediaType, mediaSubtype));
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
