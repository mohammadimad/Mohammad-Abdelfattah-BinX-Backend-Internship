using Day04.Domain;

namespace Day04.Service
{
    public class BookService : IBookService
    {
        private readonly List<Book<int>> _books = new List<Book<int>>
        {
            new Book<int> { Id = 1, Title = "Clean Code", Author = "Robert C. Martin" },
            new Book<int> { Id = 2, Title = "Design Patterns", Author = "Erich Gamma" },
            new Book<int> { Id = 3, Title = "Refactoring", Author = "Martin Fowler" }
        };

        public IReadOnlyList<Book<int>> GetBooks()
        {
            return _books.AsReadOnly();
        }

        public Book<int>? GetBookById(int id)
        {
            return _books.FirstOrDefault(b => b.Id == id);
        }
    }
}
