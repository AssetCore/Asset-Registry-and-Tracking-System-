namespace Notification.Infrastructure.Configuration;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";
    
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    
    // Queue names for different notification types
    public string WarrantyExpiryQueue { get; set; } = "warranty_expiry_notifications";
    public string MaintenanceDueQueue { get; set; } = "maintenance_due_notifications";
    public string AssetAssignmentQueue { get; set; } = "asset_assignment_notifications";
}

public class EmailSettings
{
    public const string SectionName = "Email";
    
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Asset Management System";
}

public class SmsSettings
{
    public const string SectionName = "SMS";
    
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}
