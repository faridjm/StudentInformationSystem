using Microsoft.Extensions.Configuration;
using SIS.BLL.Services.Interfaces;
using System.Net;
using System.Net.Mail;

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
            var enableSsl = bool.Parse(_configuration["Email:EnableSsl"]!);

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var message = new MailMessage
            {
                From = new MailAddress(from, "SIS — Student Information System"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(to);

            await client.SendMailAsync(message);
        }

        public async Task SendPasswordResetAsync(string to, string resetLink)
        {
            var subject = "Şifrə Sıfırlama — SIS";

            var htmlBody = $@"
        <!DOCTYPE html>
        <html>
        <body style='font-family: Arial, sans-serif; background: #f4f4f4; padding: 20px;'>
            <div style='max-width: 600px; margin: 0 auto; background: #fff; 
                        border-radius: 10px; padding: 30px;'>
                <h2 style='color: #e94560;'>Şifrə Sıfırlama</h2>
                <p>Şifrənizi sıfırlamaq üçün aşağıdakı düyməyə basın:</p>
                <a href='{resetLink}' 
                   style='display: inline-block; background: #e94560; color: #fff; 
                          padding: 12px 24px; border-radius: 8px; text-decoration: none;
                          font-weight: bold; margin: 20px 0;'>
                    Şifrəni Sıfırla
                </a>
                <p style='color: #666; font-size: 13px;'>
                    Bu link 24 saat ərzində etibarlıdır.<br>
                    Əgər bu sorğunu siz etməmisinizsə, bu emaili nəzərə almayın.
                </p>
                <hr style='border: none; border-top: 1px solid #eee;'>
                <p style='color: #999; font-size: 12px;'>
                    SIS — Student Information System
                </p>
            </div>
        </body>
        </html>";

            await SendAsync(to, subject, htmlBody);
        }

        public async Task SendWelcomeAsync(string to, string fullName, string password)
        {
            var subject = "Xoş Gəldiniz — SIS";

            var htmlBody = $@"
        <!DOCTYPE html>
        <html>
        <body style='font-family: Arial, sans-serif; background: #f4f4f4; padding: 20px;'>
            <div style='max-width: 600px; margin: 0 auto; background: #fff; 
                        border-radius: 10px; padding: 30px;'>
                <h2 style='color: #e94560;'>Xoş Gəldiniz, {fullName}!</h2>
                <p>SIS sisteminə qeydiyyatınız tamamlandı.</p>
                <div style='background: #f8f8f8; border-radius: 8px; padding: 16px; margin: 20px 0;'>
                    <p style='margin: 0;'><strong>Email:</strong> {to}</p>
                    <p style='margin: 8px 0 0;'><strong>Şifrə:</strong> {password}</p>
                </div>
                <p style='color: #e94560; font-size: 13px;'>
                    ⚠️ Giriş etdikdən sonra şifrənizi dəyişdirməyiniz tövsiyə olunur.
                </p>
                <hr style='border: none; border-top: 1px solid #eee;'>
                <p style='color: #999; font-size: 12px;'>
                    SIS — Student Information System
                </p>
            </div>
        </body>
        </html>";

            await SendAsync(to, subject, htmlBody);
        }
    }
}
