using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting; 
namespace CardiacMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] DTOs.RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.IsSuccess)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Registration failed.",
                detail: result.Message,
                instance: HttpContext.Request.Path);
        }
        return Ok(new { Message = result.Message });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    [EnableRateLimiting("StrictLoginPolicy")]
    public async Task<IActionResult> Login([FromBody] DTOs.LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.IsSuccess)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication failed.",
                detail: result.Message,
                instance: HttpContext.Request.Path);
        }

        return Ok(new
        {
            Token = result.Token,
            RefreshToken = result.RefreshToken,
            Message = result.Message
        });
    }
    // POST: api/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if (!result.IsSuccess)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Token refresh failed.",
                detail: result.Message,
                instance: HttpContext.Request.Path);
        }
        return Ok(new { Token = result.Token, RefreshToken = result.RefreshToken, Message = result.Message });
    }
}
