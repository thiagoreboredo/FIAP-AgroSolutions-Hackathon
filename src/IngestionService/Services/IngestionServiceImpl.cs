using IngestionService.DTOs;
using MassTransit;
using Shared.Messages;

namespace IngestionService.Services;

public class IngestionServiceImpl : IIngestionService
{
    private readonly IPublishEndpoint _publishEndpoint;

    public IngestionServiceImpl(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task IngestAsync(SensorDataRequest request)
    {
        var message = new SensorDataMessage(
            request.TalhaoId,
            request.SoilMoisture,
            request.Temperature,
            request.Precipitation,
            DateTime.UtcNow
        );

        await _publishEndpoint.Publish(message);
    }
}
