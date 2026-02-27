using AlertService.Models;

namespace AlertService.Services;

public class AlertServiceImpl : IAlertService
{
    private readonly ILogger<AlertServiceImpl> _logger;

    public AlertServiceImpl(ILogger<AlertServiceImpl> logger)
    {
        _logger = logger;
    }

    public Task ProcessAlertAsync(DroughtAlert alert)
    {
        _logger.LogWarning(
            "DROUGHT ALERT: Talhao {TalhaoId} has soil moisture at {SoilMoisture}% (below 30% threshold). Detected at {DetectedAt}. {Message}",
            alert.TalhaoId,
            alert.SoilMoisture,
            alert.DetectedAt,
            alert.Message
        );
        return Task.CompletedTask;
    }
}
