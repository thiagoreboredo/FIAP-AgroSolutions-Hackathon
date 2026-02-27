namespace PropertyService.DTOs;

public record CreatePropertyRequest(string Name, string Location, double TotalAreaHectares, Guid OwnerId);
