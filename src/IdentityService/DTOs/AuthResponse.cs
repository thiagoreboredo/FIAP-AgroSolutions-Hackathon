namespace IdentityService.DTOs;

public record AuthResponse(string Token, string Email, string Name);
