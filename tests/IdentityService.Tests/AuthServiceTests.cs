using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace IdentityService.Tests;

public class AuthServiceTests
{
    private IdentityDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new IdentityDbContext(options);
    }

    private IConfiguration CreateConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "TestSecretKey12345678901234567890123456789012" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnAuthResponse_WhenValidRequest()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);
        var request = new RegisterRequest("João Silva", "joao@teste.com", "senha123");

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("joao@teste.com", result.Email);
        Assert.Equal("João Silva", result.Name);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenEmailAlreadyExists()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);
        var request = new RegisterRequest("João Silva", "joao@teste.com", "senha123");

        // Act
        await service.RegisterAsync(request);
        var result = await service.RegisterAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponse_WhenValidCredentials()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);
        var registerRequest = new RegisterRequest("Maria Santos", "maria@teste.com", "senha456");
        await service.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest("maria@teste.com", "senha456");

        // Act
        var result = await service.LoginAsync(loginRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("maria@teste.com", result.Email);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenInvalidPassword()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);
        var registerRequest = new RegisterRequest("Maria Santos", "maria@teste.com", "senha456");
        await service.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest("maria@teste.com", "senhaErrada");

        // Act
        var result = await service.LoginAsync(loginRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        var loginRequest = new LoginRequest("naoexiste@teste.com", "qualquersenha");

        // Act
        var result = await service.LoginAsync(loginRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAccountAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);
        var registerRequest = new RegisterRequest("Pedro Costa", "pedro@teste.com", "senha789");
        await service.RegisterAsync(registerRequest);
        var user = await context.Users.FirstAsync(u => u.Email == "pedro@teste.com");

        // Act
        var result = await service.DeleteAccountAsync(user.Id);

        // Assert
        Assert.True(result);
        Assert.False(await context.Users.AnyAsync(u => u.Email == "pedro@teste.com"));
    }

    [Fact]
    public async Task DeleteAccountAsync_ShouldReturnFalse_WhenUserNotFound()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var config = CreateConfiguration();
        var service = new AuthService(context, config);

        // Act
        var result = await service.DeleteAccountAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
