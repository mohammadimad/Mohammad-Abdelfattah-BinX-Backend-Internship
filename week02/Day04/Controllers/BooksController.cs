using Day04.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Day04.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private static readonly List<Book> _books = new List<Book>
        {
            new Book { Id = 101, Title = "Clean Code", Author = "Robert C. Martin", Price = 45.00m },
            new Book { Id = 102, Title = "C# in Depth", Author = "Jon Skeet", Price = 50.00m },
            new Book { Id = 103, Title = "Design Patterns", Author = "Erich Gamma", Price = 55.50m }
        };

        [HttpGet]
        public IActionResult GetBooks()
        {
            return Ok(_books);
        }


        [HttpGet("{id}")]
        public IActionResult GetBookById(int id)
        {
            var book = _books.FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                return NotFound($"Book with ID {id} was not found.");
            }

            return Ok(book);
        }
    }
}
