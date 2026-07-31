using Day04.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Day04.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        // 2. حقن الخدمة عبر البناء (Constructor Injection)
        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public IActionResult GetAllBooks()
        {
            var books = _bookService.GetBooks();
            return Ok(books);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetBookById(int id)
        {
            var book = _bookService.GetBookById(id);

            if (book == null)
            {
                return NotFound(new { Message = $"Book with ID {id} was not found." });
            }

            return Ok(book);
        }
    }
}
