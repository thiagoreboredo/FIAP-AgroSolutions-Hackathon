using IngestionService.DTOs;

namespace IngestionService.Services;

public interface IIngestionService
{
    Task IngestAsync(SensorDataRequest request);
}
