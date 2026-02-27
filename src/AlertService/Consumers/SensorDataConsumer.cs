using AlertService.Models;
using AlertService.Services;
using MassTransit;
using Shared.Messages;

namespace AlertService.Consumers;

public class SensorDataConsumer : IConsumer<SensorDataMessage>
{
    private readonly IAlertService _alertService;
    private readonly ILogger<SensorDataConsumer> _logger;
    private readonly IPropertyStatusClient _statusClient;
    private const double DroughtThreshold = 30.0;

    public SensorDataConsumer(
        IAlertService alertService,
        ILogger<SensorDataConsumer> logger,
        IPropertyStatusClient statusClient)
    {
        _alertService = alertService;
        _logger = logger;
        _statusClient = statusClient;
    }

    public async Task Consume(ConsumeContext<SensorDataMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Received sensor data for Talhao {TalhaoId}: Moisture={SoilMoisture}%, Temp={Temperature}°C, Precipitation={Precipitation}mm",
            message.TalhaoId, message.SoilMoisture, message.Temperature, message.Precipitation
        );

        if (message.SoilMoisture < DroughtThreshold)
        {
            var alert = new DroughtAlert
            {
                TalhaoId = message.TalhaoId,
                SoilMoisture = message.SoilMoisture,
                DetectedAt = message.Timestamp,
                Message = $"Soil moisture critically low at {message.SoilMoisture}%. Immediate irrigation recommended."
            };

            await _alertService.ProcessAlertAsync(alert);
            await _statusClient.UpdateTalhaoStatusAsync(message.TalhaoId, "Drought Alert");
        }
        else
        {
            await _statusClient.UpdateTalhaoStatusAsync(message.TalhaoId, "Normal");
        }
    }
}
