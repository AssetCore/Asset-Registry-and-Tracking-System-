namespace Notification.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

public class SlackService : ISlackService
{
    private readonly SlackSettings _slackSettings;
    private readonly ILogger<SlackService> _logger;
    private readonly HttpClient _httpClient;

    public SlackService(IOptions<SlackSettings> slackSettings, ILogger<SlackService> logger, HttpClient httpClient)
    {
        _slackSettings = slackSettings.Value;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> SendSlackMessageAsync(string channel, string message, string? userName = null)
    {
        try
        {
            if (string.IsNullOrEmpty(_slackSettings.WebhookUrl))
            {
                _logger.LogWarning("Slack webhook URL not configured. Message not sent.");
                return false;
            }

            if (string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("Attempted to send empty Slack message");
                return false;
            }

            // Use the provided channel or fall back to default
            var targetChannel = string.IsNullOrEmpty(channel) ? _slackSettings.DefaultChannel : channel;
            
            // Ensure channel starts with # or @
            if (!targetChannel.StartsWith("#") && !targetChannel.StartsWith("@"))
            {
                targetChannel = "#" + targetChannel;
            }

            var slackMessage = new SlackMessage
            {
                Channel = targetChannel,
                Text = message,
                Username = userName ?? _slackSettings.BotName,
                IconEmoji = _slackSettings.BotIconEmoji,
                Attachments = CreateAttachment(message)
            };

            var json = JsonSerializer.Serialize(slackMessage, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var response = await _httpClient.PostAsJsonAsync(_slackSettings.WebhookUrl, slackMessage);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Slack message sent successfully to {Channel}", targetChannel);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to send Slack message to {Channel}. Status: {Status}, Error: {Error}", 
                    targetChannel, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Slack message to {Channel}", channel);
            return false;
        }
    }

    private static SlackAttachment[] CreateAttachment(string message)
    {
        var color = message.Contains("URGENT") ? "danger" : 
                   message.Contains("REMINDER") ? "warning" : "good";

        return new[]
        {
            new SlackAttachment
            {
                Color = color,
                Text = message,
                Footer = "Asset Management System",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            }
        };
    }
}

// Slack message models
public class SlackMessage
{
    public string Channel { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string IconEmoji { get; set; } = string.Empty;
    public SlackAttachment[]? Attachments { get; set; }
}

public class SlackAttachment
{
    public string Color { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Footer { get; set; } = string.Empty;
    public long Timestamp { get; set; }
}