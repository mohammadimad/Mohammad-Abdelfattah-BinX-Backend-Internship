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

    public AuthService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config, AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
        _context = context;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var userExists = await _userManager.FindByEmailAsync(request.Email);
        if (userExists != null)
        {
            return new AuthResponse(false, "Email already registered.");
        }

        var user = new IdentityUser { UserName = request.Email, Email = request.Email };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponse(false, $"Registration failed: {errors}");
        }

        var roleExists = await _roleManager.RoleExistsAsync(request.Role);
        if (!roleExists)
        {
            return new AuthResponse(false, "Specified role does not exist.");
        }

        await _userManager.AddToRoleAsync(user, request.Role);
        return new AuthResponse(true, "User registered successfully.");
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponse(false, "Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return new AuthResponse(false, "Invalid email or password.");
        }

        // توليد الرمزين معاً وحفظ الـ Refresh Token في قاعدة البيانات
        return await GenerateTokenPairAsync(user);
    }

    // Method to refresh tokens
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
            var expiryClaim = principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp);
            if (!long.TryParse(expiryClaim?.Value, out var utcExpiryDate))
            {
                return new AuthResponse(false, "Invalid access token.");
            }

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

            var jti = principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrWhiteSpace(jti))
            {
                return new AuthResponse(false, "Invalid access token.");
            }

            if (storedToken.JwtId != jti) return new AuthResponse(false, "Token ID mismatch.");

            if (storedToken.ExpiryDate < DateTime.UtcNow)
            {
                return new AuthResponse(false, "Refresh token has expired.");
            }
           // Mark the old token as "user" and generate a completely new token pair for increased security
            storedToken.IsUsed = true;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            return await GenerateTokenPairAsync(user!);
        }
        catch (SecurityTokenException)
        {
            return new AuthResponse(false, "Invalid access token.");
        }
        catch (ArgumentException)
        {
            return new AuthResponse(false, "Invalid access token.");
        }
    }

    //method to generate access and refresh tokens
    private async Task<AuthResponse> GenerateTokenPairAsync(IdentityUser user)
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
        await _context.SaveChangesAsync();

        return new AuthResponse(true, "Tokens generated successfully.", jwtToken, refreshToken.Token);
    }
}
