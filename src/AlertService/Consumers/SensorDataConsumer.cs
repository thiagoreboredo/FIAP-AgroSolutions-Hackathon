using AlertService.Models;
using AlertService.Services;
using MassTransit;
using Prometheus;
using Shared.Messages;

namespace AlertService.Consumers;

public class SensorDataConsumer : IConsumer<SensorDataMessage>
{
    private readonly IAlertService _alertService;
    private readonly ILogger<SensorDataConsumer> _logger;
    private readonly IPropertyStatusClient _statusClient;
    private const double DroughtThreshold = 30.0;

    // 1. Declaração das métricas do Grafana
    private static readonly Gauge DroughtAlertActiveGauge = Metrics.CreateGauge(
        "agrosolutions_drought_alert_active",
        "Status de alerta de seca (1 = Alerta, 0 = Normal)",
        new GaugeConfiguration { LabelNames = new[] { "talhao_id" } });

    private static readonly Counter DroughtAlertsTotalCounter = Metrics.CreateCounter(
        "agrosolutions_drought_alerts_total",
        "Total de alertas de seca disparados",
        new CounterConfiguration { LabelNames = new[] { "talhao_id" } });

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
        var talhaoId = message.TalhaoId.ToString();

        _logger.LogInformation(
            "Received sensor data for Talhao {TalhaoId}: Moisture={SoilMoisture}%, Temp={Temperature}°C, Precipitation={Precipitation}mm",
            message.TalhaoId, message.SoilMoisture, message.Temperature, message.Precipitation
        );

        if (message.SoilMoisture < DroughtThreshold)
        {
            // 2. Registra o alerta nas métricas
            DroughtAlertActiveGauge.WithLabels(talhaoId).Set(1);
            DroughtAlertsTotalCounter.WithLabels(talhaoId).Inc(); // Incrementa o totalizador

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
            // 3. Retorna o status para Normal nas métricas
            DroughtAlertActiveGauge.WithLabels(talhaoId).Set(0);

            await _statusClient.UpdateTalhaoStatusAsync(message.TalhaoId, "Normal");
        }
    }
}