using IngestionService.DTOs;
using IngestionService.Services;
using MassTransit;
using Moq;
using Shared.Messages;
using Xunit;

namespace IngestionService.Tests;

public class IngestionServiceTests
{
    [Fact]
    public async Task IngestAsync_ShouldPublishSensorDataMessage()
    {
        // Arrange
        var mockPublishEndpoint = new Mock<IPublishEndpoint>();
        mockPublishEndpoint
            .Setup(p => p.Publish(It.IsAny<SensorDataMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new IngestionServiceImpl(mockPublishEndpoint.Object);
        var request = new SensorDataRequest(Guid.NewGuid(), 25.5, 32.0, 0.0);

        // Act
        await service.IngestAsync(request);

        // Assert
        mockPublishEndpoint.Verify(
            p => p.Publish(It.Is<SensorDataMessage>(m =>
                m.TalhaoId == request.TalhaoId &&
                m.SoilMoisture == request.SoilMoisture &&
                m.Temperature == request.Temperature &&
                m.Precipitation == request.Precipitation
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task IngestAsync_ShouldPublishMessageWithCurrentTimestamp()
    {
        // Arrange
        var mockPublishEndpoint = new Mock<IPublishEndpoint>();
        SensorDataMessage? capturedMessage = null;
        mockPublishEndpoint
            .Setup(p => p.Publish(It.IsAny<SensorDataMessage>(), It.IsAny<CancellationToken>()))
            .Callback<SensorDataMessage, CancellationToken>((msg, _) => capturedMessage = msg)
            .Returns(Task.CompletedTask);

        var service = new IngestionServiceImpl(mockPublishEndpoint.Object);
        var request = new SensorDataRequest(Guid.NewGuid(), 15.0, 28.0, 5.0);
        var beforeIngest = DateTime.UtcNow;

        // Act
        await service.IngestAsync(request);

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.True(capturedMessage.Timestamp >= beforeIngest);
    }
}
