using JewelryMS.Domain.DTOs.Auth;


namespace JewelryMS.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}