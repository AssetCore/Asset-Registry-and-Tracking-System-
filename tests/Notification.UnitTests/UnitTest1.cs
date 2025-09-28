namespace Notification.UnitTests;

using Moq;
using Microsoft.Extensions.Logging;
using Notification.Application.Services;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Enums;

public class NotificationServiceTests
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ISmsService> _mockSmsService;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockSmsService = new Mock<ISmsService>();
        _mockLogger = new Mock<ILogger<NotificationService>>();
        
        _notificationService = new NotificationService(
            _mockEmailService.Object,
            _mockSmsService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SendNotificationAsync_EmailOnly_CallsEmailService()
    {
        // Arrange
        var message = new NotificationMessage
        {
            Channel = NotificationChannel.Email,
            EmailAddress = "test@example.com",
            Subject = "Test Subject",
            Body = "Test Body"
        };

        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _notificationService.SendNotificationAsync(message);

        // Assert
        Assert.True(result);
        _mockEmailService.Verify(x => x.SendEmailAsync("test@example.com", "Test Subject", "Test Body", ""), Times.Once);
        _mockSmsService.Verify(x => x.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendNotificationAsync_SmsOnly_CallsSmsService()
    {
        // Arrange
        var message = new NotificationMessage
        {
            Channel = NotificationChannel.SMS,
            PhoneNumber = "+1234567890",
            Body = "Test Body"
        };

        _mockSmsService.Setup(x => x.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _notificationService.SendNotificationAsync(message);

        // Assert
        Assert.True(result);
        _mockSmsService.Verify(x => x.SendSmsAsync("+1234567890", "Test Body"), Times.Once);
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendNotificationAsync_Both_CallsBothServices()
    {
        // Arrange
        var message = new NotificationMessage
        {
            Channel = NotificationChannel.Both,
            EmailAddress = "test@example.com",
            PhoneNumber = "+1234567890",
            Subject = "Test Subject",
            Body = "Test Body"
        };

        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockSmsService.Setup(x => x.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _notificationService.SendNotificationAsync(message);

        // Assert
        Assert.True(result);
        _mockEmailService.Verify(x => x.SendEmailAsync("test@example.com", "Test Subject", "Test Body", ""), Times.Once);
        _mockSmsService.Verify(x => x.SendSmsAsync("+1234567890", "Test Body"), Times.Once);
    }

    [Fact]
    public async Task ProcessWarrantyExpiryAsync_CreatesCorrectMessage()
    {
        // Arrange
        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockSmsService.Setup(x => x.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _notificationService.ProcessWarrantyExpiryAsync(
            "ASSET-001",
            "Test Laptop",
            "owner@example.com",
            "+1234567890",
            "John Doe",
            DateTime.UtcNow.AddDays(5),
            5);

        // Assert
        _mockEmailService.Verify(x => x.SendEmailAsync(
            "owner@example.com",
            "Warranty Expiry Alert: Test Laptop",
            It.Is<string>(body => body.Contains("Test Laptop") && body.Contains("5 day(s)")),
            "John Doe"), Times.Once);
    }
}
