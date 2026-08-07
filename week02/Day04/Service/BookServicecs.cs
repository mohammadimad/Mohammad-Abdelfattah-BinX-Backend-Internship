using Day04.Domain;

namespace Day04.Service
{
    public class BookService : IBookService
    {
        private readonly List<Book> _books = new List<Book>
        {
            new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin" },
            new Book { Id = 2, Title = "Design Patterns", Author = "Erich Gamma" },
            new Book { Id = 3, Title = "Refactoring", Author = "Martin Fowler" }
        };

        public IReadOnlyList<Book> GetBooks()
        {
            return _books.AsReadOnly();
        }

        public Book? GetBookById(int id)
        {
            return _books.FirstOrDefault(b => b.Id == id);
        }
    }
}
