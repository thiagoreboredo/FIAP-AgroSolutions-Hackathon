using Microsoft.EntityFrameworkCore;
using PropertyService.Data;
using PropertyService.DTOs;
using PropertyService.Models;

namespace PropertyService.Services;

public class PropertyServiceImpl : IPropertyService
{
    private readonly PropertyDbContext _context;

    public PropertyServiceImpl(PropertyDbContext context)
    {
        _context = context;
    }

    public async Task<List<Property>> GetAllPropertiesAsync(Guid ownerId)
        => await _context.Properties.Where(p => p.OwnerId == ownerId).Include(p => p.Talhoes).ToListAsync();

    public async Task<Property?> GetPropertyByIdAsync(Guid id)
        => await _context.Properties.Include(p => p.Talhoes).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Property> CreatePropertyAsync(CreatePropertyRequest request)
    {
        var property = new Property
        {
            Name = request.Name,
            Location = request.Location,
            TotalAreaHectares = request.TotalAreaHectares,
            OwnerId = request.OwnerId
        };
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task<Property?> UpdatePropertyAsync(Guid id, CreatePropertyRequest request)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property is null) return null;

        property.Name = request.Name;
        property.Location = request.Location;
        property.TotalAreaHectares = request.TotalAreaHectares;
        property.OwnerId = request.OwnerId;
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task<bool> DeletePropertyAsync(Guid id)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property is null) return false;
        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePropertiesByOwnerAsync(Guid ownerId)
    {
        var properties = await _context.Properties
            .Include(p => p.Talhoes)
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync();

        if (!properties.Any()) return false;

        _context.Properties.RemoveRange(properties);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Talhao>> GetTalhoesByPropertyIdAsync(Guid propertyId)
        => await _context.Talhoes.Where(t => t.PropertyId == propertyId).ToListAsync();

    public async Task<Talhao?> GetTalhaoByIdAsync(Guid id)
        => await _context.Talhoes.Include(t => t.Property).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Talhao> CreateTalhaoAsync(CreateTalhaoRequest request)
    {
        var talhao = new Talhao
        {
            Name = request.Name,
            AreaHectares = request.AreaHectares,
            CropType = request.CropType,
            PropertyId = request.PropertyId
        };
        _context.Talhoes.Add(talhao);
        await _context.SaveChangesAsync();
        return talhao;
    }

    public async Task<Talhao?> UpdateTalhaoAsync(Guid id, CreateTalhaoRequest request)
    {
        var talhao = await _context.Talhoes.FindAsync(id);
        if (talhao is null) return null;

        talhao.Name = request.Name;
        talhao.AreaHectares = request.AreaHectares;
        talhao.CropType = request.CropType;
        talhao.PropertyId = request.PropertyId;
        await _context.SaveChangesAsync();
        return talhao;
    }

    public async Task<bool> DeleteTalhaoAsync(Guid id)
    {
        var talhao = await _context.Talhoes.FindAsync(id);
        if (talhao is null) return false;
        _context.Talhoes.Remove(talhao);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TalhaoStatusResponse?> GetTalhaoStatusAsync(Guid talhaoId)
    {
        var talhao = await _context.Talhoes.FindAsync(talhaoId);
        if (talhao is null) return null;

        return new TalhaoStatusResponse(talhao.Id, talhao.Name, talhao.Status, talhao.StatusUpdatedAt);
    }

    public async Task<bool> UpdateTalhaoStatusAsync(Guid talhaoId, string status)
    {
        if (status != "Normal" && status != "Drought Alert")
            throw new ArgumentException("Status must be 'Normal' or 'Drought Alert'.", nameof(status));

        var talhao = await _context.Talhoes.FindAsync(talhaoId);
        if (talhao is null) return false;

        talhao.Status = status;
        talhao.StatusUpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
