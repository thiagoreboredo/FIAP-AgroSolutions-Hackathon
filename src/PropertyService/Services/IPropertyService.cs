using PropertyService.DTOs;
using PropertyService.Models;

namespace PropertyService.Services;

public interface IPropertyService
{
    Task<List<Property>> GetAllPropertiesAsync(Guid ownerId);
    Task<Property?> GetPropertyByIdAsync(Guid id);
    Task<Property> CreatePropertyAsync(CreatePropertyRequest request);
    Task<Property?> UpdatePropertyAsync(Guid id, CreatePropertyRequest request);
    Task<bool> DeletePropertyAsync(Guid id);
    Task<bool> DeletePropertiesByOwnerAsync(Guid ownerId);

    Task<List<Talhao>> GetTalhoesByPropertyIdAsync(Guid propertyId);
    Task<Talhao?> GetTalhaoByIdAsync(Guid id);
    Task<Talhao> CreateTalhaoAsync(CreateTalhaoRequest request);
    Task<Talhao?> UpdateTalhaoAsync(Guid id, CreateTalhaoRequest request);
    Task<bool> DeleteTalhaoAsync(Guid id);
    Task<TalhaoStatusResponse?> GetTalhaoStatusAsync(Guid talhaoId);
    Task<bool> UpdateTalhaoStatusAsync(Guid talhaoId, string status);
}
