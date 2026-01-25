using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using JewelryMS.Application.DTOs.Auth;
using JewelryMS.Domain.Interfaces;
using JewelryMS.Domain.Entities;
using Npgsql; // Added to catch specific database errors

namespace JewelryMS.API.Controllers;

[ApiController]
[Route("api/[controller]")] 
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserRepository userRepo, IConfiguration config, ILogger<AuthController> logger)
    {
        _userRepo = userRepo;
        _config = config;
        _logger = logger;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { message = "Auth Controller is reachable!", status = "Success" });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        // Robust Validation: Handle nulls and empty strings for better security
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and Password are required." });
        }

        try 
        {
            // 1. Fetch user from DB
            var user = await _userRepo.ValidateUserAsync(request.Email, request.Password);
            
            if (user == null) 
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
                // Structured Error: Helping the frontend handle failure cleanly
                return Unauthorized(new { message = "Invalid Email or Password" });
            }

            // 2. Generate the JWT Token
            var token = GenerateJwtToken(user); 

            // 3. Structured Success Response
            return Ok(new AuthResponse { 
                Token = token, 
                Email = user.Email, 
                Role = user.Role.ToString(),
                ShopId = user.ShopId ?? Guid.Empty ,
                Id = user.Id 
            });
        }
        // Handle specific Database Connectivity issues (e.g., Postgres is down)
        catch (NpgsqlException ex)
        {
            _logger.LogCritical(ex, "Database connection failure for {Email}", request.Email);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "The database is currently unavailable. Please try again later." });
        }
        // General Error Handling
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An internal server error occurred during authentication." });
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

        var jwtKey = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey)) throw new InvalidOperationException("JWT Key not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30), // Token will expire in 1 minute
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}