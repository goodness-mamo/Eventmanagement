using EventManagementApi.DTOs.Auth;

namespace EventManagementApi.Services.Interfaces;

public interface IAuthService
{
    Task<(AuthResponse? Response, string? Error)>
        RegisterAsync(RegisterRequest request);

    Task<(AuthResponse? Response, string? Error)>
        LoginAsync(LoginRequest request);
}
