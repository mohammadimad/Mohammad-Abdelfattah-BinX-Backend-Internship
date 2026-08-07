using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Day01.Controllers
{
    [Route("api/v2/products")]
    [ApiController]
    public class ProductsV2Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            // العقد الجديد يعيد تفاصيل أكثر (اسم + سعر)
            return Ok(new[] {
                new { Name = "Laptop", Price = 1200 },
                new { Name = "Mouse", Price = 25 }
            });
        }


    }
}
