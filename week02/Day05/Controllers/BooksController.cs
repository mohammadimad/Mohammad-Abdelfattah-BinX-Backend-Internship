using Day01.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Day01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

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

        [HttpPost]
        public IActionResult Create([FromBody] Book newBook)
        {
            if (newBook == null)
            {
                return BadRequest(new { Message = "Invalid book data." });
            }

            var createdBook = _bookService.CreateBook(newBook);

            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Book updatedBook)
        {
            if (updatedBook == null)
            {
                return BadRequest(new { Message = "Invalid book data." });
            }

            var isUpdated = _bookService.UpdateBook(id, updatedBook);

            if (!isUpdated)
            {
                return NotFound(new { Message = $"Book with ID {id} was not found." });
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var isDeleted = _bookService.DeleteBook(id);

            if (!isDeleted)
            {
                return NotFound(new { Message = $"Book with ID {id} was not found." });
            }

            return NoContent();
        }
    }
}