


namespace Day04
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
        public decimal Price { get; }
        public bool IsBorrowed { get; private set; }
        public Book(int id, string title, decimal price)
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
        public static void listFun()
        {
            Console.WriteLine("Enter your tasks ");
            List<string> tasks = new List<string>();
            string task = "";
            while (true)
            {

                task = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(task))
                    continue;
                if (task.ToLower() == "exit")
                    break;
                tasks.Add(task);
            }
            Console.WriteLine("your tasks: ");
            foreach (string lis in tasks)
            {
                Console.WriteLine(lis);
            }
            tasks.Sort();
            Console.WriteLine("Task for sort: ");
            foreach (string lis in tasks)
            {
                Console.WriteLine(lis);
            }
        }
        public static void fruitSearch()
        {
            List<string> fruit = new List<string> { "orange", "apple", "banaa", "tomtot", "watermelne" };
            Console.WriteLine("Enter fruit ");
            string value = Console.ReadLine();
            if (fruit.Contains(value))
                fruit.Remove(value);
            else
                fruit.Add("Not Found");

            foreach (string lis in fruit)
            {
                Console.WriteLine(lis);
            }
        }
        public static void numbersFilter()
        {
                int[] arr = { 5, 12, 8, 20, 3, 15 };
            List<int> bigNumbers = new List<int>();
            foreach (int value in arr)
            {
                if (value > 10)
                    bigNumbers.Add(value);
            }
            foreach (int values in bigNumbers)
            {
                Console.WriteLine(values);
            }
        }
        public static void gradeStudent()
        {
            Dictionary<string, int> student = new Dictionary<string, int>();
            student.Add("mohammad", 99);
            student.Add("Ahmad", 54);
            student.Add("Bader", 93);
            string name;
            int dgree;
            Console.WriteLine("Please enter your name: ");
            name = Console.ReadLine();

            if (student.TryGetValue(name, out int grade))
            {
                Console.WriteLine($"Student: {name}, Grade: {grade}");
            }
        }
        public static void employeeDictionary()
        {
            Dictionary<int, string> emplyee = new Dictionary<int, string>();
            string name = "";
            for (int i = 1; i <= 3; i++)
            {
                Console.Write("Enter your name: ");
                name = Console.ReadLine();
                emplyee.Add(i, name);
            }
            Console.WriteLine("Information Employee");
            foreach (var emp in emplyee)
            {
                Console.WriteLine($"Id: {emp.Key}, Name: {emp.Value}");
            }
            Console.WriteLine("Enter your id want'ed delete: ");
            Console.Write("Enter ID to delete: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                // التحقق مما إذا كان الحذف قد تم فعلياً [5]
                if (emplyee.Remove(id))
                    Console.WriteLine($"Employee with ID {id} has been removed.");
                else
                    Console.WriteLine("ID not found in our records.");
            }
            else
            {
                Console.WriteLine("Invalid ID format!");
            }

            Console.WriteLine("Information Employee");
            foreach (var emp in emplyee)
            {
                Console.WriteLine($"Id: {emp.Key}, Name: {emp.Value}");
            }
        }
        static async Task Main(string[] args)
        {
            //listFun();
            //numbersFilter();
            //fruitSearch();
            //gradeStudent();
            //employeeDictionary();

            List<Book> books = new List<Book>
            {
                new Book(1, "Dhikr",20),
                new Book(2, "Dhikr",20),
                new Book(3, "Inner Dimensions of Islamic Worship",40),
                new Book(4, "Clean Code",30),
                new Book(5, "Design Patterns",20),
                new Book(6, "C# in Depth",10),
                new Book(7, "Sincere Devotion",60),
                new Book(8, "Introduction to Algorithms",45)
            };




            var expensiveBooks = books.Where(b => b.Title.Length > 5).ToList();
            Console.WriteLine("Books with a greater number of letters than 5");
            foreach (var b in expensiveBooks)
            {
                Console.WriteLine($"ID: {b.Id} (Title: {b.Title})");
            }
            Console.WriteLine();
            books[1].MarkAsReturned();
            var availableTitles = books.Where(b => !b.IsBorrowed).Select(b => b.Title).ToList();
            Console.WriteLine("List of books currently available for borrowing:");
            foreach (var title in availableTitles)
            {
                Console.WriteLine(title);
            }
            Console.WriteLine();

            decimal averagePrice = books.Average(b => b.Price);
            Console.WriteLine($"Average book prices : {averagePrice}\n");



            Console.WriteLine("Data retrieval...");
            string asyncResult = await FetchDatabaseStatusAsync();
            Console.WriteLine($"Result: {asyncResult}\n");
             try
            {
                Console.Write("Enter id ");
                string input = Console.ReadLine();

                int bookId = int.Parse(input);

                var foundBook = books.FirstOrDefault(b => b.Id == bookId);
                if (foundBook != null)
                {
                    Console.WriteLine($"found: {foundBook.Title} price {foundBook.Price:C}");
                }
                else
                {
                    Console.WriteLine("not found");
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Erorr message: {ex.Message}");
            }
            Console.ReadKey();
        }


        public static async Task<string> FetchDatabaseStatusAsync()
        {
            await Task.Delay(2000);
            return "The database was contacted";
        }

    }
}