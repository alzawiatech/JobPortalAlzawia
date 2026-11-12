
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace DubaiJobs.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(
            IConfiguration configuration,
            ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(
            string email,
            string subject,
            string htmlMessage)
        {
            // Read SMTP settings
            var smtpServer =
                _configuration["EmailSettings:SmtpServer"];

            var smtpPort =
                _configuration.GetValue<int>("EmailSettings:SmtpPort");

            var senderName =
                _configuration["EmailSettings:SenderName"];

            var senderEmail =
                _configuration["EmailSettings:SenderEmail"];

            var username =
                _configuration["EmailSettings:Username"];

            var password =
                _configuration["EmailSettings:Password"];

            // Validate SMTP configuration
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "Titan SMTP password is missing. Check User Secrets.");
            }

            if (string.IsNullOrWhiteSpace(smtpServer) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                smtpPort <= 0)
            {
                throw new InvalidOperationException(
                    "SMTP configuration is incomplete.");
            }

            // Create email message
            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(senderName, senderEmail));

            message.To.Add(
                new MailboxAddress(email, email));

            message.Subject = subject;

            message.Body = new BodyBuilder
            {
                HtmlBody = htmlMessage
            }.ToMessageBody();

            // Create SMTP client
            using var client = new SmtpClient();

            // Set timeout to 30 seconds
            client.Timeout = 30000;

            try
            {
                _logger.LogInformation(
                    "STEP 1: Connecting to Titan SMTP server {Server}:{Port}",
                    smtpServer,
                    smtpPort);

                await client.ConnectAsync(
                    smtpServer,
                    smtpPort,
                    SecureSocketOptions.SslOnConnect);

                _logger.LogInformation(
                    "STEP 2: SMTP connection successful.");

                await client.AuthenticateAsync(
                    username,
                    password);

                _logger.LogInformation(
                    "STEP 3: SMTP authentication successful.");

                _logger.LogInformation(
                    "STEP 4: Sending email to {Recipient}.",
                    email);

                var response = await client.SendAsync(message);

                _logger.LogInformation(
                    "STEP 5: Titan SMTP accepted the email. Response: {Response}",
                    response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Email sending failed for recipient {Recipient}.",
                    email);

                throw;
            }
            finally
            {
                if (client.IsConnected)
                {
                    try
                    {
                        await client.DisconnectAsync(true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "SMTP disconnect failed.");
                    }
                }
            }
        }
    }
}
