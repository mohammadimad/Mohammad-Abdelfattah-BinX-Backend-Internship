namespace Day04.Domain
{
    public interface IBookService
    {
            IReadOnlyList<Book> GetBooks();
            Book? GetBookById(int id);
    }
}
