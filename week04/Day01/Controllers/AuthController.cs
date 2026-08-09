using Day03.Domains.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
namespace Day03.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
       
        public AuthController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
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