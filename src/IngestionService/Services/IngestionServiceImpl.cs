using IngestionService.DTOs;
using MassTransit;
using Prometheus;
using Shared.Messages;

namespace IngestionService.Services;

public class IngestionServiceImpl : IIngestionService
{
    private readonly IPublishEndpoint _publishEndpoint;

    private static readonly Gauge SoilMoistureGauge = Metrics.CreateGauge(
        "agrosolutions_soil_moisture_percent",
        "Nivel de umidade do solo",
        new GaugeConfiguration { LabelNames = new[] { "talhao_id", "talhao_name" } });

    private static readonly Gauge TemperatureGauge = Metrics.CreateGauge(
        "agrosolutions_temperature_celsius",
        "Temperatura em graus Celsius",
        new GaugeConfiguration { LabelNames = new[] { "talhao_id", "talhao_name" } });

    private static readonly Gauge PrecipitationGauge = Metrics.CreateGauge(
        "agrosolutions_precipitation_mm",
        "Volume de precipitacao",
        new GaugeConfiguration { LabelNames = new[] { "talhao_id", "talhao_name" } });

    public IngestionServiceImpl(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task IngestAsync(SensorDataRequest request)
    {
        var talhaoId = request.TalhaoId.ToString();
        var talhaoName = $"Talhão {talhaoId[..8]}"; 

        SoilMoistureGauge.WithLabels(talhaoId, talhaoName).Set(request.SoilMoisture);
        TemperatureGauge.WithLabels(talhaoId, talhaoName).Set(request.Temperature);
        PrecipitationGauge.WithLabels(talhaoId, talhaoName).Set(request.Precipitation);

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