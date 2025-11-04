namespace Notification.Infrastructure.Services;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Configuration;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, string? toName = null)
    {
        try
        {
            if (string.IsNullOrEmpty(to))
            {
                _logger.LogWarning("Attempted to send email with empty recipient address");
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress(toName ?? to, to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = body,
                HtmlBody = ConvertToHtml(body)
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Connect to the SMTP server
            await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, 
                _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            // Authenticate if credentials are provided
            if (!string.IsNullOrEmpty(_emailSettings.UserName) && !string.IsNullOrEmpty(_emailSettings.Password))
            {
                await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
            }

            // Send the email
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Recipient} with subject '{Subject}'", to, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient} with subject '{Subject}'", to, subject);
            return false;
        }
    }

    private static string ConvertToHtml(string plainText)
    {
        // Convert plain text to basic HTML format
        var html = plainText
            .Replace("\n\n", "</p><p>")
            .Replace("\n", "<br/>")
            .Replace("URGENT:", "<strong style='color: red;'>URGENT:</strong>")
            .Replace("REMINDER:", "<strong style='color: orange;'>REMINDER:</strong>");

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Asset Management Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 20px; }}
        .content {{ padding: 15px; }}
        .footer {{ background-color: #f8f9fa; padding: 10px; border-radius: 5px; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Asset Management System Notification</h2>
        </div>
        <div class='content'>
            <p>{html}</p>
        </div>
        <div class='footer'>
            <p>This is an automated message from the Asset Management System. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
    }
}