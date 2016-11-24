using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace CalcApp
{
    public class Calc
    {
        private static double num1;
        private static double num2;

        static void Main(string[] args)
        {

            Console.WriteLine("Directory Info:   " + Environment.GetEnvironmentVariable("foo"));

            Console.WriteLine("Enter first number:");
            string input1 = Console.ReadLine();
            Console.WriteLine("Enter second number:");
            string input2 = Console.ReadLine();
            try
            {
                num1 = Convert.ToDouble(input1);
                num2 = Convert.ToDouble(input2);
            }
            catch
            {
                Console.WriteLine("Invalid input. Enter only numbers... ");
                return;
            }
            Console.WriteLine("Choose operation: +) Add -) Substract *) Multiply /) Divide: ");
            string operation = Console.ReadLine();
            double output;
            Calc cal1 = new Calc();
            // Console.WriteLine("{0} {1} {2} ", input1, operation, input2);
            switch (operation)
            {
                case "+":
                    output = cal1.Add(num1, num2);
                    break;

                default:
                    Console.WriteLine("Invalid operation...");
                    Console.ReadKey();
                    return;
            }

            Console.WriteLine("{0} {1} {2} = {3}", input1, operation, input2, output.ToString());
            Console.ReadKey();
        }
        public double Add(double num1, double num2)
        {
            //making sure this test takes between 0.5 - 3 sec to run
            Random random = new Random();
            Thread.Sleep(random.Next(500, 3000));

            //bug here to make sure than add test fails
            return num1 + num2 + 2;
        }
        public double Substract(double num1, double num2)
        {
            //making sure this test takes between 0.5 - 3 sec to run
            Random random = new Random();
            Thread.Sleep(random.Next(500, 3000));

            return num1 - num2;
        }
        public double Multiply(double num1, double num2)
        {
            //making sure this test takes between 0.5 - 3 sec to run
            Random random = new Random();
            Thread.Sleep(random.Next(500, 3000));

            return num1 * num2;
        }

        public double Divide(double num1, double num2)
        {
            //making sure this test takes between 0.5 - 3 sec to run
            Random random = new Random();
            Thread.Sleep(random.Next(500, 3000));

            if (num2 == 0)
            {
                throw new DivideByZeroException("Division by zero not allowed in this kingdom...");
            }
            return num1 / num2;
        }
    }
}
