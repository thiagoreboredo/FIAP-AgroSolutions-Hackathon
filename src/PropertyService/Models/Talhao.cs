namespace PropertyService.Models;

public class Talhao
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public double AreaHectares { get; set; }
    public string CropType { get; set; } = string.Empty;
    public Guid PropertyId { get; set; }
    public Property? Property { get; set; }
    public string Status { get; set; } = "Normal";
    public DateTime StatusUpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
