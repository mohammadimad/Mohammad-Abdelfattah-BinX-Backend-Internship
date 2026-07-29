using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Day04.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet]

        public IActionResult GetUser()
        {
            var user = new[] { "Mohammad", "Sara", "Ahmad" };
            return Ok(user);
        }
        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = new[] { "Mohammad", "Sara", "Ahmad" };
            if (id < 0 || id >= user.Length)
            {
                return NotFound();
            }
            return Ok(user[id]);
        }


    }
}
