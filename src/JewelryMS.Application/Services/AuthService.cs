using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using JewelryMS.Domain.DTOs.Auth;
using JewelryMS.Domain.Interfaces.Repositories;
using JewelryMS.Domain.Interfaces.Services;
using JewelryMS.Domain.Entities;
using Npgsql;

namespace JewelryMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepo, IConfiguration config, ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _config = config;
        _logger = logger;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            // 1. Validate User via Repository
            var user = await _userRepo.ValidateUserAsync(request.Email, request.Password);
            
            if (user == null)
            {
                _logger.LogWarning("Failed login attempt: {Email}", request.Email);
                return null;
            }

            // 2. Generate Token
            var token = GenerateJwtToken(user);

            // 3. Return DTO
            return new AuthResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString(),
                ShopId = user.ShopId ?? Guid.Empty,
                Id = user.Id
            };
        }
        catch (NpgsqlException ex)
        {
            _logger.LogCritical(ex, "Database failure during login for {Email}", request.Email);
            throw; // Re-throw to let the controller handle the status code
        }
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),        
            new Claim("shop_id", user.ShopId?.ToString() ?? "") 
        };

        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}