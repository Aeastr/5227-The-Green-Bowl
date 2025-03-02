using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TheGreenBowl.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string from, string subject, string body)
        {
            // In a real application, this would connect to an SMTP server or email API
            // For demonstration purposes, we'll just log the email details
            
            _logger.LogInformation("========== DEMO EMAIL ==========");
            _logger.LogInformation($"To: {to}");
            _logger.LogInformation($"From: {from}");
            _logger.LogInformation($"Subject: {subject}");
            _logger.LogInformation($"Body: {body}");
            _logger.LogInformation("===============================");
            
            _logger.LogInformation("Email sent successfully (demo mode)");
        }
    }
}