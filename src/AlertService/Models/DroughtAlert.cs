namespace AlertService.Models;

public class DroughtAlert
{
    public Guid TalhaoId { get; set; }
    public double SoilMoisture { get; set; }
    public DateTime DetectedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
