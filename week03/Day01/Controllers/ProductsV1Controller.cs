using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Day01.Controllers
{
    [Route("api/v1/products")]
    [ApiController]
    public class ProductsV1Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            // العقد (Contract) القديم يعيد نصوصاً فقط
            return Ok(new[] { "Laptop", "Mouse" });
        }
    }
}

