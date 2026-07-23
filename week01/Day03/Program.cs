
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
namespace Day03
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
    public class Book
    {
        public int Id { get; }
        public string Title { get; }
        public bool IsBorrowed { get; private set; }
        public Book(int id, string title)
        {
            Id = id;
            Title = title;
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
    public class Member
    {
        private int _idMember;
        private bool _IsActive;
        public string Name { get; set; }
        private readonly List<Book> _borrowedBooks;

        public IReadOnlyCollection<Book> BorrowedBooks => _borrowedBooks.AsReadOnly();

        public Member(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Member name cannot be empty.");

            _idMember = id;
            Name = name;
            _IsActive = true;
            _borrowedBooks = new List<Book>();
        }

        public BorrowResponse BorrowBook(Book book)
        {

            book.MarkAsBorrowed();
            _borrowedBooks.Add(book);
            return new BorrowResponse(this.Name, book.Title, DateTime.Now);
        }

        public decimal ReturnBook(Book book, int dayLate, IFeeCalculator feeCalculator)
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

    internal class Program
    {
        static void Main(string[] args)
        {
            var book = new Book(101, "C# ");
            var member = new Member(1, "mohammad");
            BorrowResponse receipt = member.BorrowBook(book);

            member.BorrowBook(book);


            Console.WriteLine($"Book '{book.Title}' borrowed by {member.Name}.");
            decimal fee = member.ReturnBook(book, 2, new RegularBookFee());
            Console.WriteLine($"Book returned. Late fee is: ${fee}");
            Console.WriteLine($"Receipt Details: {receipt}");
            member.Print("mohammad");
            member.Print();
        }
    }
}