
using  System;

using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Day02
{
   
    internal class Program
    {
        static void GetType()
        {
            List<string> list = new List<string>();
            string name = "Mohammad";
            int[] arr = new int[5];

            int num = 24;
            float num2 = 4114.3f;
            double num3 = 241d;
            Console.WriteLine("Task One: ");
            Console.WriteLine("Value Types: ");
            Console.WriteLine(num.GetType());
            Console.WriteLine(num2.GetType());
            Console.WriteLine(num3.GetType());

            Console.WriteLine("Reference Types: ");
            Console.WriteLine(list.GetType());
            Console.WriteLine(name.GetType());
            Console.WriteLine(arr.GetType());
        }
        static void EditValue()
        {
            int original = 31;
            int copy = original;
            Console.WriteLine("======== \nTask Two: ");
            Console.WriteLine("Before edit");
            Console.WriteLine("original value " + original);
            Console.WriteLine("copy value " + copy);

            copy = 41;
            Console.WriteLine("After edit");
            Console.WriteLine("original value " + original);
            Console.WriteLine("copy value " + copy);
        }
        static void EditRefrence()
        {
            int[] original = { 12, 31 };
            int[] copy = original;

            Console.WriteLine("Before edit");
            Console.WriteLine("original value " + original[0]);
            Console.WriteLine("copy value " + copy[0]);

            copy[0] = 99;
            Console.WriteLine("After edit");
            Console.WriteLine("original value " + original[0]);
            Console.WriteLine("copy value " + copy[0]);
        }
        static string StudentLevel(int score) => score switch
        {
            int n when n >= 90 && n <= 100 => "A - Excellent",
            int n when n >= 80 && n < 90 => "B - Very Good",
            int n when n >= 70 && n < 80 => "C - Good",
            int n when n >= 60 && n < 70 => "D - Pass",
            int n when n >= 0 && n < 60 => "F - Fail",
         _        => "Invalid Score" // الخيار الافتراضي لأي قيمة خارج النطاق
        };
        public static string GetUserAddress(string question, string defAccuce)
        {
            Console.Write(question);

            string? userInput = Console.ReadLine();

            if (userInput == null || userInput == "")
            {
                return defAccuce;
            }
            else
            {
                return userInput;
            }
        }
        static void Main(string[] args)
        {

            GetType();
            EditValue();
            EditRefrence();
            string address = GetUserAddress("Enter your address ", "Tulkarm, Palestion");
            Console.WriteLine($"The address is: {address}");
        }

    }
}
