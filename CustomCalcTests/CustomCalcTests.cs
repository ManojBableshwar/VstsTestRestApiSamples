using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcApp;

namespace CustomCalcTests
{
    class Program
    {
        static void Main(string[] args)
        {
            string resultsFilePath;
            if (args.Length == 0)
            {
                resultsFilePath = "Results.csv";
            }
            else
            {
                resultsFilePath = args[0];
            }

            System.IO.StreamWriter resultsFile = new System.IO.StreamWriter(resultsFilePath);

            Calc cal1 = new Calc();
            double mathResult;
            string output, startTime, endTime;

            // simple addition test - this is expeced to fail since there is a bug in add logic... 
            try
            {
                //record start time of the test... 
                startTime = DateTime.Now.ToString();

                //call the method to be tested and record the result... 
                mathResult = cal1.Add(3, 4);

                //record end time of the test... 
                endTime = DateTime.Now.ToString();

                //write the test result... 
                if (mathResult == 7)
                {
                    output = "TestAdd,Passed,Tested 3 + 4;  Expected: 7; Actual: " + mathResult + ";,," + startTime + "," + endTime;
                    Console.WriteLine(output);
                    resultsFile.WriteLine(output);
                }
                else
                {
                    output = "TestAdd,Failed,Tested 3 + 4;  Expected: 7; Actual: " + mathResult + ";,," + startTime + "," + endTime;
                    Console.WriteLine(output);
                    resultsFile.WriteLine(output);
                }
            }
            catch (Exception e)
            {
                output = "TestAdd,Error,Tested 3 + 4; Expected: 7; Actual: Exception found; see stack trace.," + e + "," + DateTime.Now.ToString() + "," + DateTime.Now.ToString();
                Console.WriteLine(output);
                resultsFile.WriteLine(output);
            }


            // simple subtraction test - this is will pass...  
            try
            {
                //record start time of the test... 
                startTime = DateTime.Now.ToString();

                //call the method to be tested and record the result... 
                mathResult = cal1.Substract(9, 4);

                //record end time of the test... 
                endTime = DateTime.Now.ToString();

                //write the test result... 
                if (mathResult == 5)
                {
                    output = "TestSub,Passed,Tested 9 - 4;  Expected: 5; Actual: " + mathResult + ";,," + startTime + "," + endTime;
                    Console.WriteLine(output);
                    resultsFile.WriteLine(output);
                }
                else
                {
                    output = "TestSub,Failed,Tested 9 - 4;  Expected: 5; Actual: " + mathResult + ";,," + startTime + "," + endTime;
                    Console.WriteLine(output);
                    resultsFile.WriteLine(output);
                }
            }
            catch (Exception e)
            {
                output = "TestSub,Error,Tested 9 - 4;  Expected: 5; Actual: Exception found; see stack trace.," + e + "," + DateTime.Now.ToString() + "," + DateTime.Now.ToString();
                Console.WriteLine(output);
                resultsFile.WriteLine(output);
            }

            // simple multiplication test - this is will pass...  
            try
            {
                //record start time of the test... 
                startTime = DateTime.Now.ToString();

                //call the method to be tested and record the result... 
                mathResult = cal1.Multiply(2, 4);

                //record end time of the test... 
                endTime = DateTime.Now.ToString();

                //write the test result... 
                if (mathResult == 8)
                {
                    output = "TestMul,Passed,Tested 2 * 4;  Expected: 8; Actual: " + mathResult + ";,," + startTime + "," + endTime;
                    Console.WriteLine(output);
                    resultsFile.WriteLine(output);
                }
                else
                {
                    output = "TestMul,Passed,Tested 2 * 4;  Expected: 8; Actual: " + mathResult + ";,," + startTime + "," + endTime;
                    Console.WriteLine(output);
                    resultsFile.WriteLine(output);
                }
            }
            catch (Exception e)
            {
                output = "TestMul,Error,Tested 2 * 4;  Expected: 8; Actual: Exception found; see stack trace.," + e + "," + DateTime.Now.ToString() + "," + DateTime.Now.ToString();
                Console.WriteLine(output);
                resultsFile.WriteLine(output);
            }


            // simple division test - this is will throw div by zero exception and report error...  
            try
            {
                //record start time of the test... 
                startTime = DateTime.Now.ToString();

                //call the method to be tested and record the result... 
                mathResult = cal1.Divide(5, 0);

                //record end time of the test... 
                endTime = DateTime.Now.ToString();

                //write the test result... comparision is a no-op since we hope to catch an exception...
                if (mathResult == 8)
                {
                    output = "TestDiv,Passed,Tested 5 / 0;  Expected: infinity :-) ; Actual: " + mathResult + ";,," + startTime + "," + endTime;
                    Console.WriteLine(output);
                    resultsFile.WriteLine(output);
                }
                else
                {
                    output = "TestDiv,Passed,Tested 5 / 0;  Expected: infinity :-) ; Actual: " + mathResult + ";,," + startTime + "," + endTime;
                    Console.WriteLine(output);
                    resultsFile.WriteLine(output);
                }
            }
            catch (Exception e)
            {

                output = "TestDiv,Error,Tested 5 * 0;  Expected: infinity :-) ; Actual: Exception found; see stack trace.," + e.Message + "," + DateTime.Now.ToString() + "," + DateTime.Now.ToString();
                output = output.Replace("\n", "\t"); // else, newlines from stack trace will mess with csv file... 
                output = output.Replace("\n", "\t"); // else, newlines from stack trace will mess with csv file... 
                Console.WriteLine(output);
                resultsFile.WriteLine(output);
            }

            resultsFile.Close();

            //Console.ReadKey();
        }
    }
}
