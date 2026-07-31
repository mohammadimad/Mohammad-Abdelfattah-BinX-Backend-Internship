namespace Day04.Domain
{
    public class Book<T>
    {
        public T Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
    }
}
