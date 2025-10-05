# Notification Service

The Notification Service is part of the Asset Registry and Tracking System microservices architecture. It handles sending email and Slack notifications for asset-related events such as warranty expiry, maintenance due dates, and asset assignments.

## 🏗️ Architecture

The service follows **Clean Architecture** principles with the following layers:

- **Domain**: Core business entities, enums, and events
- **Application**: Business logic, interfaces, and use cases
- **Infrastructure**: External integrations (RabbitMQ, Email, Slack)
- **API**: REST endpoints and controllers

## 🚀 Features

- **Email Notifications**: Send HTML and text emails via SMTP
- **Slack Notifications**: Send messages to Slack channels via webhooks
- **RabbitMQ Integration**: Consume messages from other microservices
- **Multiple Notification Types**:
  - Warranty expiry alerts
  - Maintenance due reminders
  - Asset assignment notifications
- **Health Checks**: Monitor service status
- **Unit Tests**: Comprehensive test coverage

## 📋 Prerequisites

- **.NET 8.0 SDK**
- **RabbitMQ Server** (for message queuing)
- **SMTP Server** (for email notifications) 
- **Slack Workspace** (for Slack notifications)

## ⚙️ Configuration

Update the `appsettings.json` or `appsettings.Development.json` files with your configuration:

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "WarrantyExpiryQueue": "warranty_expiry_notifications",
    "MaintenanceDueQueue": "maintenance_due_notifications",
    "AssetAssignmentQueue": "asset_assignment_notifications"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "UserName": "outdrax@gmail.com",
    "Password": "onxoplkepauebylq",
    "FromEmail": "outdrax@gmail.com",
    "FromName": "Asset Management System"
  },
  "Slack": {
    "WebhookUrl": "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK"
  }
}
```

## 🔧 Setup Instructions

### 1. Install Dependencies

```bash
# Navigate to the notification service directory
cd src/Services/Notification/Notification.Api

# Restore NuGet packages
dotnet restore
```

### 2. Setup Email (Gmail Example)

1. Enable **2-Factor Authentication** on your Gmail account
2. Generate an **App Password**: [Google App Passwords](https://myaccount.google.com/apppasswords)
3. Use the app password (not your regular password) in the configuration

### 3. Setup Slack Webhooks

1. Go to your Slack workspace
2. Navigate to **Apps** > **Manage** > **Custom Integrations** > **Incoming Webhooks**
3. Click **Add to Slack**
4. Choose the channel where notifications will be sent
5. Copy the **Webhook URL** and add it to your configuration
6. Optionally customize the bot name and icon

**Alternative method:**
1. Go to [Slack API](https://api.slack.com/apps)
2. Create a new app or select existing one
3. Go to **Incoming Webhooks** and activate it
4. Create a new webhook for your workspace
5. Copy the webhook URL

### 4. Setup RabbitMQ

```bash
# Using Docker
docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# Or install locally from https://www.rabbitmq.com/download.html
```

## 🏃‍♂️ Running the Service

### Development Mode

```bash
cd src/Services/Notification/Notification.Api
dotnet run
```

The service will start on:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`

### Production Mode

```bash
dotnet build -c Release
dotnet run -c Release
```

## 📡 API Endpoints

### Health Check
```http
GET /health
```

### Send Manual Notification
```http
POST /api/notification/send
Content-Type: application/json

{
  "type": 1,
  "channel": 3,
  "emailAddress": "user@example.com",
  "slackChannel": "#general",
  "recipientName": "John Doe",
  "subject": "Test Notification",
  "body": "This is a test message",
  "assetId": "ASSET-001",
  "assetName": "Test Asset"
}
```

### Warranty Expiry Notification
```http
POST /api/notification/warranty-expiry
Content-Type: application/json

{
  "assetId": "LAPTOP-001",
  "assetName": "Dell Laptop",
  "ownerEmail": "john@company.com",
  "slackChannel": "#general",
  "ownerName": "John Doe",
  "warrantyExpiryDate": "2025-12-31T00:00:00Z",
  "daysUntilExpiry": 30
}
```

### Swagger Documentation

When running in development mode, access the Swagger UI at:
`https://localhost:5001/swagger`

## 🔄 RabbitMQ Integration

The service automatically consumes messages from these queues:
- `warranty_expiry_notifications`
- `maintenance_due_notifications`  
- `asset_assignment_notifications`

### Message Format Examples

**Warranty Expiry Event:**
```json
{
  "assetId": "LAPTOP-001",
  "assetName": "Dell Laptop",
  "ownerEmail": "john@company.com",
  "slackChannel": "#general", 
  "ownerName": "John Doe",
  "warrantyExpiryDate": "2025-12-31T00:00:00Z",
  "daysUntilExpiry": 30
}
```

## 🧪 Testing

### Run Unit Tests
```bash
cd tests/Notification.UnitTests
dotnet test
```

### Run All Tests
```bash
# From solution root
dotnet test
```

## 🐳 Docker Support

### Build Docker Image
```bash
docker build -f src/Services/Notification/Notification.Api/Dockerfile -t notification-service .
```

### Run with Docker Compose
```yaml
version: '3.8'
services:
  notification-service:
    image: notification-service
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - RabbitMQ__HostName=rabbitmq
    depends_on:
      - rabbitmq
      
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
```

## 🔧 Troubleshooting

### Common Issues

1. **RabbitMQ Connection Failed**
   - Ensure RabbitMQ server is running
   - Check connection settings in appsettings.json

2. **Email Sending Failed**  
   - Verify SMTP settings
   - Check if using correct app password for Gmail
   - Ensure "Less secure app access" is enabled (if not using app password)

3. **Slack Sending Failed**
   - Verify Slack webhook URL
   - Check if webhook is active in Slack workspace
   - Ensure channel name is correct (e.g., "#general")

### Logs

Check application logs for detailed error information:
```bash
# Enable detailed logging
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Notification": "Debug"
  }
}
```

## 🤝 Integration with Other Services

Other microservices can send notifications by publishing messages to RabbitMQ queues:

```csharp
// Example: Asset Management Service sending warranty expiry notification
var warrantyEvent = new AssetWarrantyExpiryEvent
{
    AssetId = "LAPTOP-001",
    AssetName = "Dell Laptop",
    OwnerEmail = "john@company.com",
    SlackChannel = "#general",
    OwnerName = "John Doe", 
    WarrantyExpiryDate = DateTime.UtcNow.AddDays(30),
    DaysUntilExpiry = 30
};

// Publish to RabbitMQ
channel.BasicPublish("", "warranty_expiry_notifications", null, 
    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(warrantyEvent)));
```

## 📚 Next Steps

1. **Add Database Support**: Store notification history and retry logic
2. **Add More Channels**: Microsoft Teams, Push notifications, Discord
3. **Implement Templates**: Configurable message templates
4. **Add Analytics**: Track delivery rates and engagement
5. **Implement Circuit Breaker**: Handle external service failures gracefully

## 🐛 Known Limitations

- No notification history storage (stateless)
- No delivery status tracking
- Limited retry mechanism
- No template system for messages

This service provides a solid foundation for notifications in your Asset Registry and Tracking System!