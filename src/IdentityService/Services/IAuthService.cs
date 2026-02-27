using IdentityService.DTOs;

namespace IdentityService.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<bool> DeleteAccountAsync(Guid userId);
}
