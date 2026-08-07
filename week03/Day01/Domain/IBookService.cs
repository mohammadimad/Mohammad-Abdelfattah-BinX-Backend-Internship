namespace Day01.Domain
{
    public interface IBookService
    {
        IReadOnlyList<Book> GetBooks();
        Book? GetBookById(int id);
        Book CreateBook(Book newBook);
        bool UpdateBook(int id, Book updatedBook);
        bool DeleteBook(int id);
    }
}