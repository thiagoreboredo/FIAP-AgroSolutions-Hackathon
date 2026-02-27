namespace Shared.Messages;

public record SensorDataMessage(
    Guid TalhaoId,
    double SoilMoisture,
    double Temperature,
    double Precipitation,
    DateTime Timestamp
);
