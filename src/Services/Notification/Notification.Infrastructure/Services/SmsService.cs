namespace Notification.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

public class SmsService : ISmsService
{
    private readonly SmsSettings _smsSettings;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IOptions<SmsSettings> smsSettings, ILogger<SmsService> logger)
    {
        _smsSettings = smsSettings.Value;
        _logger = logger;

        // Initialize Twilio
        if (!string.IsNullOrEmpty(_smsSettings.AccountSid) && !string.IsNullOrEmpty(_smsSettings.AuthToken))
        {
            TwilioClient.Init(_smsSettings.AccountSid, _smsSettings.AuthToken);
        }
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                _logger.LogWarning("Attempted to send SMS with empty phone number");
                return false;
            }

            if (string.IsNullOrEmpty(_smsSettings.AccountSid) || string.IsNullOrEmpty(_smsSettings.AuthToken))
            {
                _logger.LogWarning("SMS service not configured. AccountSid or AuthToken is missing");
                return false;
            }

            // Clean the phone number (remove spaces, dashes, etc.)
            var cleanPhoneNumber = CleanPhoneNumber(phoneNumber);
            
            // Truncate message if too long (SMS limit is typically 160 characters)
            var truncatedMessage = message.Length > 160 
                ? message.Substring(0, 157) + "..." 
                : message;

            var messageResource = await MessageResource.CreateAsync(
                body: truncatedMessage,
                from: new Twilio.Types.PhoneNumber(_smsSettings.FromNumber),
                to: new Twilio.Types.PhoneNumber(cleanPhoneNumber)
            );

            if (messageResource.Status == MessageResource.StatusEnum.Sent ||
                messageResource.Status == MessageResource.StatusEnum.Queued)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber} with SID {MessageSid}", 
                    cleanPhoneNumber, messageResource.Sid);
                return true;
            }
            else
            {
                _logger.LogWarning("SMS failed to send to {PhoneNumber}. Status: {Status}, Error: {ErrorMessage}", 
                    cleanPhoneNumber, messageResource.Status, messageResource.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    private static string CleanPhoneNumber(string phoneNumber)
    {
        // Remove all non-digit characters except the + sign
        var cleaned = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
        
        // If no country code, assume US/CA (+1)
        if (!cleaned.StartsWith("+"))
        {
            if (cleaned.Length == 10)
            {
                cleaned = "+1" + cleaned;
            }
            else if (cleaned.Length == 11 && cleaned.StartsWith("1"))
            {
                cleaned = "+" + cleaned;
            }
        }

        return cleaned;
    }
}