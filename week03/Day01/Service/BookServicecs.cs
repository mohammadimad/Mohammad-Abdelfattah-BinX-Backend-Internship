using  Day01.Domain;

namespace Day01.Service
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

        public Book CreateBook(Book newBook)
        {
            newBook.Id = _books.Any() ? _books.Max(b => b.Id) + 1 : 1;
            _books.Add(newBook);
            return newBook;
        }

        public bool UpdateBook(int id, Book updatedBook)
        {
            var existingBook = GetBookById(id);
            if (existingBook == null)
            {
                return false;
            }

            existingBook.Title = updatedBook.Title;
            existingBook.Author = updatedBook.Author;
            return true;
        }

        public bool DeleteBook(int id)
        {
            var book = GetBookById(id);
            if (book == null)
            {
                return false;
            }

            _books.Remove(book);
            return true;
        }
    }
}