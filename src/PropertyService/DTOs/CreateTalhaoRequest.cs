namespace PropertyService.DTOs;

public record CreateTalhaoRequest(string Name, double AreaHectares, string CropType, Guid PropertyId);
