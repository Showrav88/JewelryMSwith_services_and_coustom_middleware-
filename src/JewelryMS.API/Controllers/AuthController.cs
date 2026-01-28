using Microsoft.AspNetCore.Mvc;
using JewelryMS.Domain.DTOs.Auth;
using JewelryMS.Domain.Interfaces.Services;
using Npgsql;

namespace JewelryMS.API.Controllers;

[ApiController]
[Route("api/[controller]")] 
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { message = "Auth service is running." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and Password are required." });
        }

        try 
        {
            var response = await _authService.LoginAsync(request);

            if (response == null) 
            {
                return Unauthorized(new { message = "Invalid Email or Password" });
            }

            return Ok(response);
        }
        catch (NpgsqlException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Database is unavailable. Please try again later." });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An internal server error occurred." });
        }
    }
}