using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CardiacMonitor.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context; // حقن قاعدة البيانات لحفظ الرموز
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config,
        AppDbContext context,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
        _context = context;
        _logger = logger;
    }

    // Creates the identity user and role membership in one transaction.
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var userExists = await _userManager.FindByEmailAsync(request.Email);
        if (userExists != null)
        {
            return new AuthResponse(false, "Email already registered.");
        }

        var roleExists = await _roleManager.RoleExistsAsync(request.Role);
        if (!roleExists)
        {
            return new AuthResponse(false, "Specified role does not exist.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };
            var creationResult = await _userManager.CreateAsync(user, request.Password);
            if (!creationResult.Succeeded)
            {
                await transaction.RollbackAsync();
                var errors = string.Join(", ", creationResult.Errors.Select(error => error.Description));
                return new AuthResponse(false, $"Registration failed: {errors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                var errors = string.Join(", ", roleResult.Errors.Select(error => error.Description));
                return new AuthResponse(false, $"Role assignment failed: {errors}");
            }

            await transaction.CommitAsync();
            return new AuthResponse(true, "User registered successfully.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Failed login attempt: user was not found.");
            return new AuthResponse(false, "Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning(
                "Failed login attempt for user {UserId}: invalid password.",
                user.Id);
            return new AuthResponse(false, "Invalid email or password.");
        }

        // توليد الرمزين معاً وحفظ الـ Refresh Token في قاعدة البيانات
        return await GenerateTokenPairAsync(user);
    }

    // Replaces an expired token pair atomically after validating both tokens.
    public async Task<AuthResponse> RefreshTokenAsync(TokenRequest request)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        try
        {
            // 1. التحقق من صحة الـ Access Token (حتى لو كان منتهي الصلاحية برمجياً)
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // لا نفحص تاريخ الصلاحية هنا لكي نقرأه وهو منتهي
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!))
            };

            var principal = jwtTokenHandler.ValidateToken(request.AccessToken, tokenValidationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                if (!result) return new AuthResponse(false, "Invalid token algorithm.");
            }

            //Check the expiry date of the original Access Token
            var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)!.Value);
            var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(utcExpiryDate).UtcDateTime;

            if (expiryDateTime > DateTime.UtcNow)
            {
                return new AuthResponse(false, "Access token has not expired yet.");
            }

            //Verify the existence and validity of the Refresh Token in the database
            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == request.RefreshToken);
            if (storedToken == null) return new AuthResponse(false, "Refresh token does not exist.");
            if (storedToken.IsUsed) return new AuthResponse(false, "Refresh token has already been used.");
            if (storedToken.IsRevoked) return new AuthResponse(false, "Refresh token has been revoked.");

            var jti = principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)!.Value;
            if (storedToken.JwtId != jti) return new AuthResponse(false, "Token ID mismatch.");

            if (storedToken.ExpiryDate < DateTime.UtcNow)
            {
                return new AuthResponse(false, "Refresh token has expired.");
            }
            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            if (user == null)
            {
                return new AuthResponse(false, "The refresh token user no longer exists.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                storedToken.IsUsed = true;
                var response = await GenerateTokenPairAsync(user, saveChanges: false);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return response;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception exception) when (
            exception is SecurityTokenException
            or ArgumentException
            or FormatException)
        {
            _logger.LogWarning(
                "Refresh token validation failed with {ExceptionType}.",
                exception.GetType().Name);

            return new AuthResponse(false, "The token request is invalid.");
        }
    }

    // Generates and optionally persists a new access and refresh token pair.
    private async Task<AuthResponse> GenerateTokenPairAsync(
        IdentityUser user,
        bool saveChanges = true)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        //Token Generation
            var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:DurationInMinutes"]!)),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        //Refresh Token: in 7 days
   
        //Remember to save the refresh token
        var refreshToken = new RefreshToken
        {
            JwtId = token.Id,
            IsUsed = false,
            IsRevoked = false,
            UserId = user.Id,
            AddedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString()
        };

        _context.RefreshTokens.Add(refreshToken);
        if (saveChanges)
        {
            await _context.SaveChangesAsync();
        }

        return new AuthResponse(true, "Tokens generated successfully.", jwtToken, refreshToken.Token);
    }
}
