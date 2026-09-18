using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Authorization;
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
    /// <summary>Registers a Patient account and its linked clinical profile.</summary>
    /// <param name="request">Contact details and a strong password. Public registration cannot select a privileged role.</param>
    /// <response code="200">The Identity user and Patient profile were created.</response>
    /// <response code="400">Validation failed or the email is already registered.</response>
    [HttpPost("register")]
    [AllowAnonymous]
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
    /// <summary>Authenticates an existing account and issues access and refresh tokens.</summary>
    /// <param name="request">The registered email and password.</param>
    /// <response code="200">A signed access token and a single-use refresh token.</response>
    /// <response code="401">Invalid email or password.</response>
    /// <response code="429">The login attempt limit was exceeded.</response>
    [HttpPost("login")]
    [AllowAnonymous]
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
    /// <summary>Rotates a matching expired access token and unused refresh token.</summary>
    /// <param name="request">The expired access token and its original refresh token.</param>
    /// <response code="200">A replacement token pair; the old refresh token is consumed.</response>
    /// <response code="400">The pair is invalid, already used, revoked, expired, or the access token has not expired.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
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
