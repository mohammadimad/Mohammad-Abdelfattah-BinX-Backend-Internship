using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CardiacMonitor.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public AuthService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config, AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
        _context = context;
    }

    // Register the account and patient file together in one step (Transaction)
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var userExists = await _userManager.FindByEmailAsync(request.Email);
        if (userExists != null)
        {
            return new AuthResponse(false, "Email already registered.");
        }

        // Starting the Database Transaction to protect data integrity
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new IdentityUser { UserName = request.Email, Email = request.Email };

            // 1. Create an Identity account
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponse(false, $"Registration failed: {errors}");
            }

            // 2. Verification and adding the user role
            var roleExists = await _roleManager.RoleExistsAsync(request.Role);
            if (!roleExists)
            {
                return new AuthResponse(false, "Specified role does not exist.");
            }
            await _userManager.AddToRoleAsync(user, request.Role);

            // 3.If the role is "Patient", we create the patient file immediately in the same process.
            if (request.Role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patient = new Patient
                {
                    UserId = user.Id,
                    FirstName = request.FirstName ?? "New",
                    LastName = request.LastName ?? "Patient",
                    DateOfBirth = request.DateOfBirth ?? DateTime.UtcNow.AddYears(-30),
                    Gender = request.Gender ?? "Unknown",
                    ContactNumber = request.ContactNumber ?? string.Empty
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return new AuthResponse(true, "User and profile registered successfully.");
        }
        catch (Exception ex)
        {
            // If any step fails, it is immediately reversed as if nothing had happened.
            await transaction.RollbackAsync();
            return new AuthResponse(false, $"Registration failed: {ex.Message}");
        }
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

        return await GenerateTokenPairAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(TokenRequest request)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
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

            var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)!.Value);
            var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(utcExpiryDate).UtcDateTime;

            if (expiryDateTime > DateTime.UtcNow)
            {
                return new AuthResponse(false, "Access token has not expired yet.");
            }

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

            storedToken.IsUsed = true;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            return await GenerateTokenPairAsync(user!);
        }
        catch (Exception)
        {
            return new AuthResponse(false, "An error occurred while processing your request.");
        }
    }

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

        // The required feature for week seven: Obtaining the PatientId and injecting it as a Claim specifically to avoid repeated inquiries

        if (userRoles.Contains("Patient"))
        {
            var patient = await _context.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient != null)
            {
                authClaims.Add(new Claim("PatientId", patient.Id.ToString()));
            }
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:DurationInMinutes"]!)),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

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