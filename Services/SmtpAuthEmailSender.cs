using Event_Parking_Reservation_System.Configuration;
using Event_Parking_Reservation_System.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Event_Parking_Reservation_System.Services
{
    public class SmtpAuthEmailSender : IAuthEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpAuthEmailSender> _logger;

        public SmtpAuthEmailSender(
            IOptions<EmailSettings> settings,
            IConfiguration configuration,
            ILogger<SmtpAuthEmailSender> logger)
        {
            _settings = settings.Value;
            _configuration = configuration;
            _logger = logger;
        }

        public Task SendVerificationEmailAsync(string toEmail, string verificationToken)
        {
            var baseUrl = _configuration["Frontend:BaseUrl"] ?? "http://127.0.0.1:5500";
            var link = $"{baseUrl.TrimEnd('/')}/verify-email.html?token={Uri.EscapeDataString(verificationToken)}";
            return SendAsync(toEmail, "Verify your email",
                $"Welcome! Please verify your email by opening this link:\n\n{link}\n\nOr use token: {verificationToken}");
        }

        public Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var baseUrl = _configuration["Frontend:BaseUrl"] ?? "http://127.0.0.1:5500";
            var link = $"{baseUrl.TrimEnd('/')}/reset-password.html?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(toEmail)}";
            return SendAsync(toEmail, "Reset your password",
                $"Reset your password using this link:\n\n{link}\n\nOr use token: {resetToken}");
        }

        private async Task SendAsync(string toEmail, string subject, string body)
        {
            if (_settings.SimulateOnly || string.IsNullOrWhiteSpace(_settings.SmtpHost))
            {
                _logger.LogInformation("EMAIL SIMULATED to {Email} | {Subject}\n{Body}", toEmail, subject, body);
                return;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = true,
                Credentials = string.IsNullOrWhiteSpace(_settings.Username)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_settings.Username, _settings.Password)
            };

            await client.SendMailAsync(message);
        }
    }
}
