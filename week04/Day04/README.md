Here is the completed, developer-ready README.md for Day 2 in English. It is
structured directly to help you implement JWT login and token issuance quickly.

🔑 Day 2: JWT Authentication & Token Issuance

Welcome to Day 2. Today, we transition from user registration to user
authentication. We will implement a login endpoint that verifies credentials and
issues a signed JSON Web Token (JWT), and configure the authentication
middleware to validate these tokens on incoming requests.

🎯 Learning Objectives

  - Understand the structure of a JWT (Header, Payload, Signature) and the
    concept of Claims.
  - Implement a login endpoint using SignInManager and generate signed JWTs.
  - Configure JWT bearer authentication middleware in ASP.NET Core.
  - Handle token expiration and security keys correctly.

🛠️ Prerequisites & Tools

  - Completion of Day 1 (Identity database tables & Registration endpoint).
  - Postman for API testing.
  - Required NuGet Packages:
    dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

🚀 Step-by-Step Implementation

Step 1: Add JWT Configuration to appsettings.json

Add your JWT parameters to your local settings file. Ensure the signing key is
kept secure and out of source control in production.

{
  "Jwt": {
    "Issuer": "BinXTechHub",
    "Audience": "BinXTechHubUsers",
    "Key": "A_Very_Long_And_Secure_Secret_Key_With_At_Least_32_Characters!"
  }
}

Step 2: Configure JWT Authentication in Program.cs

Register the JWT Authentication middleware to decode and validate incoming
tokens on protected requests.

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Removes default 5-minute grace period for expiration
    };
});

// Enable Middleware (Order Matters!)
app.UseAuthentication();
app.UseAuthorization();

Step 3: Implement Login and Token Issuance in AuthController

Inject SignInManager<IdentityUser> to verify the user's password, generate
claims, sign the token, and return it.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<IdentityUser> userManager, 
        SignInManager<IdentityUser> signInManager, 
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return Unauthorized("Invalid credentials.");

        // Check password without locking out the account
        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded) return Unauthorized("Invalid credentials.");

        var token = GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15), // Short expiration for security
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}

🧪 Testing the Endpoint (Postman & jwt.io)

Part 1: Retrieve the Token

1.  Open Postman and create a POST request to:
    https://localhost:xxxx/api/auth/login.
2.  Under the Body tab, select raw / JSON and send the credentials created on
    Day 1:
    {
      "email": "student@binxtech.com",
      "password": "SecurePassword123!"
    }
3.  Verify that the response returns a status code of 200 OK along with a long
    encoded JWT string.

Part 2: Verify Token Claims

1.  Copy the issued token from Postman.
2.  Go to jwt.io.
3.  Paste the token into the Encoded pane.
4.  Inspect the Decoded Payload to ensure your sub (User ID), email, and
    expiration (exp) claims are present and correct.
