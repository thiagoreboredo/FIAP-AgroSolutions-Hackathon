using AlertService.Consumers;
using AlertService.Models;
using AlertService.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Messages;
using Xunit;

namespace AlertService.Tests;

public class SensorDataConsumerTests
{
    private static SensorDataConsumer CreateConsumer(
        Mock<IAlertService> mockAlertService,
        Mock<ILogger<SensorDataConsumer>> mockLogger,
        Mock<IPropertyStatusClient>? mockStatusClient = null)
    {
        mockStatusClient ??= new Mock<IPropertyStatusClient>();
        mockStatusClient
            .Setup(c => c.UpdateTalhaoStatusAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        return new SensorDataConsumer(mockAlertService.Object, mockLogger.Object, mockStatusClient.Object);
    }

    [Fact]
    public async Task Consume_ShouldTriggerAlert_WhenSoilMoistureBelowThreshold()
    {
        // Arrange
        var mockAlertService = new Mock<IAlertService>();
        mockAlertService.Setup(s => s.ProcessAlertAsync(It.IsAny<DroughtAlert>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<SensorDataConsumer>>();
        var consumer = CreateConsumer(mockAlertService, mockLogger);

        var message = new SensorDataMessage(
            Guid.NewGuid(),
            SoilMoisture: 20.0, // Below 30% threshold
            Temperature: 35.0,
            Precipitation: 0.0,
            Timestamp: DateTime.UtcNow
        );

        var mockContext = new Mock<ConsumeContext<SensorDataMessage>>();
        mockContext.Setup(c => c.Message).Returns(message);

        // Act
        await consumer.Consume(mockContext.Object);

        // Assert
        mockAlertService.Verify(
            s => s.ProcessAlertAsync(It.Is<DroughtAlert>(a =>
                a.TalhaoId == message.TalhaoId &&
                a.SoilMoisture == message.SoilMoisture
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task Consume_ShouldNotTriggerAlert_WhenSoilMoistureAboveThreshold()
    {
        // Arrange
        var mockAlertService = new Mock<IAlertService>();
        var mockLogger = new Mock<ILogger<SensorDataConsumer>>();
        var consumer = CreateConsumer(mockAlertService, mockLogger);

        var message = new SensorDataMessage(
            Guid.NewGuid(),
            SoilMoisture: 45.0, // Above 30% threshold
            Temperature: 25.0,
            Precipitation: 10.0,
            Timestamp: DateTime.UtcNow
        );

        var mockContext = new Mock<ConsumeContext<SensorDataMessage>>();
        mockContext.Setup(c => c.Message).Returns(message);

        // Act
        await consumer.Consume(mockContext.Object);

        // Assert
        mockAlertService.Verify(
            s => s.ProcessAlertAsync(It.IsAny<DroughtAlert>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Consume_ShouldTriggerAlert_WhenSoilMoistureExactlyAtBoundary()
    {
        // Arrange
        var mockAlertService = new Mock<IAlertService>();
        mockAlertService.Setup(s => s.ProcessAlertAsync(It.IsAny<DroughtAlert>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<SensorDataConsumer>>();
        var consumer = CreateConsumer(mockAlertService, mockLogger);

        var message = new SensorDataMessage(
            Guid.NewGuid(),
            SoilMoisture: 29.9, // Just below 30% threshold
            Temperature: 30.0,
            Precipitation: 0.0,
            Timestamp: DateTime.UtcNow
        );

        var mockContext = new Mock<ConsumeContext<SensorDataMessage>>();
        mockContext.Setup(c => c.Message).Returns(message);

        // Act
        await consumer.Consume(mockContext.Object);

        // Assert
        mockAlertService.Verify(
            s => s.ProcessAlertAsync(It.IsAny<DroughtAlert>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Consume_ShouldUpdateStatusToDroughtAlert_WhenSoilMoistureBelowThreshold()
    {
        // Arrange
        var mockAlertService = new Mock<IAlertService>();
        mockAlertService.Setup(s => s.ProcessAlertAsync(It.IsAny<DroughtAlert>())).Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<SensorDataConsumer>>();
        var mockStatusClient = new Mock<IPropertyStatusClient>();
        mockStatusClient.Setup(c => c.UpdateTalhaoStatusAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var consumer = CreateConsumer(mockAlertService, mockLogger, mockStatusClient);

        var talhaoId = Guid.NewGuid();
        var message = new SensorDataMessage(talhaoId, SoilMoisture: 15.0, Temperature: 38.0, Precipitation: 0.0, Timestamp: DateTime.UtcNow);
        var mockContext = new Mock<ConsumeContext<SensorDataMessage>>();
        mockContext.Setup(c => c.Message).Returns(message);

        // Act
        await consumer.Consume(mockContext.Object);

        // Assert
        mockStatusClient.Verify(c => c.UpdateTalhaoStatusAsync(talhaoId, "Drought Alert"), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldUpdateStatusToNormal_WhenSoilMoistureAboveThreshold()
    {
        // Arrange
        var mockAlertService = new Mock<IAlertService>();
        var mockLogger = new Mock<ILogger<SensorDataConsumer>>();
        var mockStatusClient = new Mock<IPropertyStatusClient>();
        mockStatusClient.Setup(c => c.UpdateTalhaoStatusAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var consumer = CreateConsumer(mockAlertService, mockLogger, mockStatusClient);

        var talhaoId = Guid.NewGuid();
        var message = new SensorDataMessage(talhaoId, SoilMoisture: 55.0, Temperature: 25.0, Precipitation: 12.0, Timestamp: DateTime.UtcNow);
        var mockContext = new Mock<ConsumeContext<SensorDataMessage>>();
        mockContext.Setup(c => c.Message).Returns(message);

        // Act
        await consumer.Consume(mockContext.Object);

        // Assert
        mockStatusClient.Verify(c => c.UpdateTalhaoStatusAsync(talhaoId, "Normal"), Times.Once);
    }

    [Fact]
    public async Task ProcessAlertAsync_ShouldLogWarning_WhenDroughtAlertReceived()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AlertServiceImpl>>();
        var service = new AlertServiceImpl(mockLogger.Object);
        var alert = new DroughtAlert
        {
            TalhaoId = Guid.NewGuid(),
            SoilMoisture = 15.0,
            DetectedAt = DateTime.UtcNow,
            Message = "Test alert"
        };

        // Act
        await service.ProcessAlertAsync(alert);

        // Assert - logger.LogWarning was called
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}
