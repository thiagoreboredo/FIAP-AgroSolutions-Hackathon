using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PropertyService.Data;
using PropertyService.DTOs;
using PropertyService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PropertyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPropertyService, PropertyServiceImpl>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PropertyDbContext>();
    db.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

var propertiesGroup = app.MapGroup("/api/properties").RequireAuthorization();

propertiesGroup.MapGet("/{ownerId:guid}", async (Guid ownerId, IPropertyService svc) =>
    Results.Ok(await svc.GetAllPropertiesAsync(ownerId)));

propertiesGroup.MapGet("/detail/{id:guid}", async (Guid id, IPropertyService svc) =>
{
    var property = await svc.GetPropertyByIdAsync(id);
    return property is null ? Results.NotFound() : Results.Ok(property);
});

propertiesGroup.MapPost("/", async (CreatePropertyRequest request, IPropertyService svc) =>
{
    var property = await svc.CreatePropertyAsync(request);
    return Results.Created($"/api/properties/detail/{property.Id}", property);
});

propertiesGroup.MapPut("/{id:guid}", async (Guid id, CreatePropertyRequest request, IPropertyService svc) =>
{
    var property = await svc.UpdatePropertyAsync(id, request);
    return property is null ? Results.NotFound() : Results.Ok(property);
});

propertiesGroup.MapDelete("/{id:guid}", async (Guid id, IPropertyService svc) =>
{
    var deleted = await svc.DeletePropertyAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

var talhoesGroup = app.MapGroup("/api/talhoes").RequireAuthorization();

talhoesGroup.MapGet("/property/{propertyId:guid}", async (Guid propertyId, IPropertyService svc) =>
    Results.Ok(await svc.GetTalhoesByPropertyIdAsync(propertyId)));

talhoesGroup.MapGet("/{id:guid}", async (Guid id, IPropertyService svc) =>
{
    var talhao = await svc.GetTalhaoByIdAsync(id);
    return talhao is null ? Results.NotFound() : Results.Ok(talhao);
});

talhoesGroup.MapPost("/", async (CreateTalhaoRequest request, IPropertyService svc) =>
{
    var talhao = await svc.CreateTalhaoAsync(request);
    return Results.Created($"/api/talhoes/{talhao.Id}", talhao);
});

talhoesGroup.MapPut("/{id:guid}", async (Guid id, CreateTalhaoRequest request, IPropertyService svc) =>
{
    var talhao = await svc.UpdateTalhaoAsync(id, request);
    return talhao is null ? Results.NotFound() : Results.Ok(talhao);
});

talhoesGroup.MapDelete("/{id:guid}", async (Guid id, IPropertyService svc) =>
{
    var deleted = await svc.DeleteTalhaoAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

talhoesGroup.MapGet("/{id:guid}/status", async (Guid id, IPropertyService svc) =>
{
    var status = await svc.GetTalhaoStatusAsync(id);
    return status is null ? Results.NotFound() : Results.Ok(status);
});

// Internal endpoint used by AlertService to update talhão status (protected by API key)
app.MapPut("/internal/talhoes/{id:guid}/status", async (Guid id, UpdateTalhaoStatusRequest request, IPropertyService svc, HttpContext http, IConfiguration config) =>
{
    var expectedKey = config["InternalApiKey"];
    var providedKey = http.Request.Headers["X-Internal-Api-Key"].FirstOrDefault();
    if (string.IsNullOrEmpty(expectedKey) || providedKey != expectedKey)
        return Results.Unauthorized();

    if (request.Status != "Normal" && request.Status != "Drought Alert")
        return Results.BadRequest(new { message = "Status must be 'Normal' or 'Drought Alert'" });

    var updated = await svc.UpdateTalhaoStatusAsync(id, request.Status);
    return updated ? Results.Ok() : Results.NotFound();
});

// LGPD: Delete all properties and talhões for a given owner
propertiesGroup.MapDelete("/owner/{ownerId:guid}", async (Guid ownerId, IPropertyService svc) =>
{
    await svc.DeletePropertiesByOwnerAsync(ownerId);
    return Results.NoContent();
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }
