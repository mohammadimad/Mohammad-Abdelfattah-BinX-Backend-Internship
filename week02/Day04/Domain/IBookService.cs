namespace Day04.Domain
{
    public interface IBookService
    {
            IReadOnlyList<Book<int>> GetBooks();
            Book<int>? GetBookById(int id);
    }
}
