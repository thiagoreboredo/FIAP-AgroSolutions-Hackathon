using System.Net.Http.Json;

namespace AlertService.Services;

public class PropertyStatusClient : IPropertyStatusClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PropertyStatusClient> _logger;

    public PropertyStatusClient(HttpClient httpClient, ILogger<PropertyStatusClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task UpdateTalhaoStatusAsync(Guid talhaoId, string status)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"/internal/talhoes/{talhaoId}/status",
                new { Status = status }
            );

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning(
                    "Failed to update talhão {TalhaoId} status to '{Status}': HTTP {StatusCode}",
                    talhaoId, status, response.StatusCode
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating talhão {TalhaoId} status to '{Status}'", talhaoId, status);
        }
    }
}
