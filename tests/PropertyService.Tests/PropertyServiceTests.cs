using Microsoft.EntityFrameworkCore;
using PropertyService.Data;
using PropertyService.DTOs;
using PropertyService.Services;
using Xunit;

namespace PropertyService.Tests;

public class PropertyServiceTests
{
    private PropertyDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PropertyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PropertyDbContext(options);
    }

    [Fact]
    public async Task CreatePropertyAsync_ShouldReturnProperty_WhenValidRequest()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PropertyServiceImpl(context);
        var ownerId = Guid.NewGuid();
        var request = new CreatePropertyRequest("Fazenda São João", "Ribeirão Preto, SP", 150.5, ownerId);

        // Act
        var result = await service.CreatePropertyAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fazenda São João", result.Name);
        Assert.Equal("Ribeirão Preto, SP", result.Location);
        Assert.Equal(150.5, result.TotalAreaHectares);
        Assert.Equal(ownerId, result.OwnerId);
    }

    [Fact]
    public async Task GetAllPropertiesAsync_ShouldReturnOnlyOwnerProperties()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PropertyServiceImpl(context);
        var ownerId1 = Guid.NewGuid();
        var ownerId2 = Guid.NewGuid();

        await service.CreatePropertyAsync(new CreatePropertyRequest("Fazenda 1", "Local 1", 100, ownerId1));
        await service.CreatePropertyAsync(new CreatePropertyRequest("Fazenda 2", "Local 2", 200, ownerId1));
        await service.CreatePropertyAsync(new CreatePropertyRequest("Fazenda 3", "Local 3", 300, ownerId2));

        // Act
        var result = await service.GetAllPropertiesAsync(ownerId1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(ownerId1, p.OwnerId));
    }

    [Fact]
    public async Task CreateTalhaoAsync_ShouldReturnTalhao_WhenValidRequest()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PropertyServiceImpl(context);
        var ownerId = Guid.NewGuid();
        var property = await service.CreatePropertyAsync(new CreatePropertyRequest("Fazenda", "Local", 100, ownerId));
        var request = new CreateTalhaoRequest("Talhão A", 25.0, "Soja", property.Id);

        // Act
        var result = await service.CreateTalhaoAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Talhão A", result.Name);
        Assert.Equal("Soja", result.CropType);
        Assert.Equal(25.0, result.AreaHectares);
        Assert.Equal(property.Id, result.PropertyId);
    }

    [Fact]
    public async Task DeletePropertyAsync_ShouldReturnTrue_WhenPropertyExists()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PropertyServiceImpl(context);
        var ownerId = Guid.NewGuid();
        var property = await service.CreatePropertyAsync(new CreatePropertyRequest("Fazenda", "Local", 100, ownerId));

        // Act
        var result = await service.DeletePropertyAsync(property.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeletePropertyAsync_ShouldReturnFalse_WhenPropertyNotFound()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PropertyServiceImpl(context);

        // Act
        var result = await service.DeletePropertyAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetTalhaoStatusAsync_ShouldReturnNormalStatus_WhenTalhaoJustCreated()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PropertyServiceImpl(context);
        var ownerId = Guid.NewGuid();
        var property = await service.CreatePropertyAsync(new CreatePropertyRequest("Fazenda", "Local", 100, ownerId));
        var talhao = await service.CreateTalhaoAsync(new CreateTalhaoRequest("Talhão B", 30.0, "Milho", property.Id));

        // Act
        var result = await service.GetTalhaoStatusAsync(talhao.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Normal", result.Status);
        Assert.Equal(talhao.Id, result.TalhaoId);
    }

    [Fact]
    public async Task UpdateTalhaoStatusAsync_ShouldReturnTrue_AndPersistStatus()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PropertyServiceImpl(context);
        var ownerId = Guid.NewGuid();
        var property = await service.CreatePropertyAsync(new CreatePropertyRequest("Fazenda", "Local", 100, ownerId));
        var talhao = await service.CreateTalhaoAsync(new CreateTalhaoRequest("Talhão C", 10.0, "Trigo", property.Id));

        // Act
        var result = await service.UpdateTalhaoStatusAsync(talhao.Id, "Drought Alert");
        var status = await service.GetTalhaoStatusAsync(talhao.Id);

        // Assert
        Assert.True(result);
        Assert.NotNull(status);
        Assert.Equal("Drought Alert", status.Status);
    }

    [Fact]
    public async Task UpdateTalhaoStatusAsync_ShouldReturnFalse_WhenTalhaoNotFound()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PropertyServiceImpl(context);

        // Act
        var result = await service.UpdateTalhaoStatusAsync(Guid.NewGuid(), "Drought Alert");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateTalhaoStatusAsync_ShouldThrow_WhenInvalidStatus()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PropertyServiceImpl(context);
        var ownerId = Guid.NewGuid();
        var property = await service.CreatePropertyAsync(new CreatePropertyRequest("Fazenda", "Local", 100, ownerId));
        var talhao = await service.CreateTalhaoAsync(new CreateTalhaoRequest("Talhão D", 5.0, "Feijão", property.Id));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdateTalhaoStatusAsync(talhao.Id, "Unknown Status"));
    }

    [Fact]
    public async Task DeletePropertiesByOwnerAsync_ShouldDeleteAll_WhenOwnerHasProperties()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PropertyServiceImpl(context);
        var ownerId = Guid.NewGuid();
        await service.CreatePropertyAsync(new CreatePropertyRequest("Fazenda 1", "Local 1", 100, ownerId));
        await service.CreatePropertyAsync(new CreatePropertyRequest("Fazenda 2", "Local 2", 200, ownerId));

        // Act
        var result = await service.DeletePropertiesByOwnerAsync(ownerId);
        var remaining = await service.GetAllPropertiesAsync(ownerId);

        // Assert
        Assert.True(result);
        Assert.Empty(remaining);
    }
}
