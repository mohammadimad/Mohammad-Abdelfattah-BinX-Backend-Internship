using System.Data;
using System.Security.AccessControl;

namespace Day01
{
    public record BorrowRequest(

     int idBook,
     int idMember,
     DateTime date);
    public record BorrowResponse(
        string nameMember,
        string nameBook,
        DateTime date
    );
    public interface IFeeCalculator
    {
        decimal CalculateFee(int dayLate);
    }
    public class RegularBookFee : IFeeCalculator
    {
        public decimal CalculateFee(int dayLate)
        {
            if (dayLate <= 0) return 0;
            return 4 * dayLate;
        }
    }
    public class RareBookFee : IFeeCalculator
    {
        public decimal CalculateFee(int dayLate)
        {
            if (dayLate <= 0) return 0;
            return 6 * dayLate;
        }
    }
    public class Book<T>
    {
        public T Id { get; }
        public string Title { get; }
        public decimal Price { get; }
        public bool IsBorrowed { get; private set; }
        public Book(T id, string title, decimal price)
        {
            Id = id;
            Title = title;
            this.Price = price;
            IsBorrowed = false;
        }
        public void MarkAsBorrowed()
        {
            IsBorrowed = true;
        }
        public void MarkAsReturned()
        {
            IsBorrowed = false;
        }



    }
    public class Member<T>
    {
        private T _idMember;
        private bool _IsActive;
        public string Name { get; set; }
        private readonly List<Book<T>> _borrowedBooks;

        public IReadOnlyCollection<Book<T>> BorrowedBooks => _borrowedBooks.AsReadOnly();

        public Member(T id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Member name cannot be empty.");

            _idMember = id;
            Name = name;
            _IsActive = true;
            _borrowedBooks = new List<Book<T>>();
        }

        public BorrowResponse BorrowBook(Book<T> book)
        {

            book.MarkAsBorrowed();
            _borrowedBooks.Add(book);
            return new BorrowResponse(this.Name, book.Title, DateTime.Now);
        }

        public decimal ReturnBook(Book<T> book, int dayLate, IFeeCalculator feeCalculator)
        {

            decimal fee = feeCalculator.CalculateFee(dayLate);

            book.MarkAsReturned();
            _borrowedBooks.Remove(book);

            return fee;
        }
        public void Print(string name)
        {
            Console.WriteLine($"thank's you {name}");
        }
        public void Print()
        {
            Console.WriteLine($"Thank's you {Name}");
        }
    }
    public class Utilities
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }
    public class Repository<T> where T : class
    {
        private readonly List<T> _item = new List<T>();

        public void Add(T item)
        {
            _item.Add(item);
        }
        public IReadOnlyList<T> GetAll()
        {
            return _item.AsReadOnly();
        }

        public T? Find(Func<T, bool> predicate)
        {
            return _item.FirstOrDefault(predicate);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(" 1. Testing Repository with Books ===");
            var bookRepo = new Repository<Book<int>>();
            bookRepo.Add(new Book<int>(101, "Clean Code", 45.0m));
            bookRepo.Add(new Book<int>(102, "Design Patterns", 55.0m));

            var foundBook = bookRepo.Find(b => b.Title == "Clean Code");
            if (foundBook != null)
            {
                Console.WriteLine($"is found book");
            }
            else
                Console.WriteLine("not found book");

            Console.WriteLine(" 2. Testing Repository with Members ===");
            var memberRepo = new Repository<Member<string>>();
            memberRepo.Add(new Member<string>("11", "Mohammad"));
            memberRepo.Add(new Member<string>("13", "Ahmad"));

            var foundMember = memberRepo.Find(m => m.Name.Equals("Mohammad"));
            if (foundMember != null)
            {
                Console.WriteLine($"is found member");
            }
            else
                Console.WriteLine("is not found member");

            Console.WriteLine("3. Testing IReadOnlyList Encapsulation ===");

            IReadOnlyList<Book<int>> allBooks = bookRepo.GetAll();
            foreach (var book in allBooks)
            {
                Console.WriteLine($"Book: {book.Title}");
            }
        }
    }
}