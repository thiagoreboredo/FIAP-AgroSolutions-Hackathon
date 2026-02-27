namespace IngestionService.DTOs;

public record SensorDataRequest(
    Guid TalhaoId,
    double SoilMoisture,
    double Temperature,
    double Precipitation
);
