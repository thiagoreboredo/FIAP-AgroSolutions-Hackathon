namespace PropertyService.DTOs;

public record TalhaoStatusResponse(Guid TalhaoId, string Name, string Status, DateTime StatusUpdatedAt);
