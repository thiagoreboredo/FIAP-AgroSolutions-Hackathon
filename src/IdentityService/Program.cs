using System.Text;
using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();

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
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    db.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();

var authGroup = app.MapGroup("/api/auth");

authGroup.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
{
    var result = await authService.RegisterAsync(request);
    if (result is null)
        return Results.Conflict(new { message = "Email already registered" });
    return Results.Ok(result);
});

authGroup.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request);
    if (result is null)
        return Results.Unauthorized();
    return Results.Ok(result);
});

var userGroup = app.MapGroup("/api/user").RequireAuthorization();

userGroup.MapDelete("/me", async (HttpContext http, IAuthService authService) =>
{
    var userIdClaim = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
        ?? http.User.FindFirst("sub");
    if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        return Results.Unauthorized();

    var deleted = await authService.DeleteAccountAsync(userId);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }
