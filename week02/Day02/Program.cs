using System;
using System.Net.NetworkInformation;
using System.Numerics;

namespace Day02
{
    internal class Program
    {

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<Order> Orders { get; set; }
            public List<Phone> phones { get; set; }

            static List<Customer> customers = new List<Customer>
        {
            new Customer
            {
                Name = "Wasef",
                phones = new List<Phone> { new Phone { phone = "Samasung" }, new Phone{phone = "Apple"} }

            },
            new Customer
            {
                Name = "Ahmad",
                phones = new List<Phone> { new Phone { phone = "Hawawa" }, new Phone{phone = "Apple"} }

            }
           };
            static public void printPhones()
            {
                var flatPhone = customers.SelectMany(x => x.phones);
                foreach (var phone in flatPhone)
                {
                    Console.WriteLine($"{phone.phone}");
                }
            }
        }

        public class Order
        {
            public int CustomerId { get; set; }
            public double Amount { get; set; }
        }
        public class Phone
        {
            public string phone;
        }

        static void Main(string[] args)
        {
            //المثال الأول
            



                   var customers = new List<Customer> {
                        new Customer {Id= 1, Name ="Mohammad"},
                        new Customer {Id = 2, Name = "Wasef"},
                        new Customer {Id = 3, Name = "Areen"},
                        new Customer {Id = 4, Name = "Baha"},
                        new Customer {Id = 5, Name = "Motez"},
                        new Customer {Id = 6, Name = "Hamaza"}
                   };
            var orders = new List<Order> {
                    new Order { CustomerId = 1, Amount = 150 },
                    new Order { CustomerId = 1, Amount = 200 },
                    new Order { CustomerId = 2, Amount = 50 },
                    new Order { CustomerId = 2, Amount = 40 },  
                    new Order { CustomerId = 3, Amount = 500 },
                    new Order { CustomerId = 3, Amount = 100 }
                };

            var topCustomers = customers.Join(orders,
               c => c.Id,
               o => o.CustomerId,
               (c, o) => new { c.Name, o.Amount }).GroupBy(joind => joind.Name)
               .Select(g => new
               {
                   g.Key,
                   TotalSpent = g.Sum(x => x.Amount)
               }).OrderByDescending(g => g.TotalSpent)
    .Where(result => result.TotalSpent > 300)
    .ToList(); 
            ;


            foreach (var item in topCustomers)
            {
                Console.WriteLine($"Customer: {item.Key}, Total: {item.TotalSpent}");
            }

            Customer.printPhones();
            List<int> prices = new List<int>() { 50, 80, 150, 200 };

            var deferredQuery = prices.Where(p => p > 100);
            var immediateQuery = prices.Where(p => p > 100).ToList();

            prices[1] = 300;
            Console.WriteLine("--- Immediate Query Results: ---");
            foreach (var immediate in immediateQuery)
            {
                Console.WriteLine(immediate);
            }

            Console.WriteLine("\n--- Deferred Query Results: ---");
            foreach (var deferred in deferredQuery)
            {
                Console.WriteLine(deferred);
            }
        }

    }
}