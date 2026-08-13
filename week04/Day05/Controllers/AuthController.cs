using Day03.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace Day03.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        private async Task<string> GenerateJWTToken(IdentityUser user)
        {
            var authClaims = new List<Claim> {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(JwtRegisteredClaimNames.Sub, user.Id), 
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
              
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.Now.AddMinutes(15),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [EnableRateLimiting("LoginPolicy")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto payload)
        {
           
            var user = await _userManager.FindByEmailAsync(payload.Email);

            
            if (user != null && await _userManager.CheckPasswordAsync(user, payload.Password))
            {

               
                var tokenString = await GenerateJWTToken(user);

                return Ok(new { Token = tokenString }); 
            }

            return Unauthorized(); 
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto payload)
        {
        var userExit = await _userManager.FindByEmailAsync(payload.Email);
        if (userExit != null) return BadRequest("User already exists!");
            var newUser = new IdentityUser
            {
                UserName = payload.Username,
                Email = payload.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };
        var result = await _userManager.CreateAsync(newUser, payload.Password);
            if (!result.Succeeded)
            {
                // إرجاع كافة الأخطاء المحددة من Identity في الـ Response
                return BadRequest(result.Errors);
            }
            

        return Ok("User created successfully!"); 
    }
}
}