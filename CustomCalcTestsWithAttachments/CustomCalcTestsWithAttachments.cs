using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcApp;
using System.IO;
using System.Diagnostics;

namespace CustomCalcTestsWithAttachments
{
    class CustomCalcTestsWithAttachments
    {
        public delegate double calcDelegate(double a, double b);

        private static TraceSource CalcTraceSource = new TraceSource("CustomCalcTestsWithAttachments");
        static void Main(string[] args)
        {



            //Pick test results file path from command line, if not specified, then use Results.csv
            string resultsFilePath;
            if (args.Length == 0)
            {
                resultsFilePath = "Results.csv";
            }
            else
            {
                resultsFilePath = args[0];
            }

            //Find out the directory of the results file.. We will use this to create test method log files or attachments
            FileInfo resultsFileInfo = new FileInfo(resultsFilePath);
            string resultsFileDir = resultsFileInfo.DirectoryName;


            System.IO.StreamWriter resultsFile = new System.IO.StreamWriter(resultsFilePath);

            Console.WriteLine("Using results file: {0} and directory {1}: to create test attachments...", resultsFilePath, resultsFileDir);

            Calc cal1 = new Calc();

            testMethod(resultsFileDir, resultsFile, cal1.Add, "TestAdd", "+", 3, 4, 7);
            testMethod(resultsFileDir, resultsFile, cal1.Substract, "TestSub", "-", 7, 4, 3);
            testMethod(resultsFileDir, resultsFile, cal1.Multiply, "TestMul", "*", 3, 4, 12);
            testMethod(resultsFileDir, resultsFile, cal1.Divide, "TestDiv", "/", 3, 0, 0);

            resultsFile.Close();

            //Console.ReadKey();
        }

        private static void testMethod(string resultsFileDir, StreamWriter resultsFile, calcDelegate testMethod, string TestName, string operation, double num1, double num2, double expectedResult)
        {
            Trace.AutoFlush = true;
            var sourceSwitch = new SourceSwitch("SourceSwitch", "Verbose");
            CalcTraceSource.Switch = sourceSwitch;
            // simple addition test - this is expeced to fail since there is a bug in add logic... 
            string traceFilePath = resultsFileDir + "/" + TestName + ".log";
            TextWriterTraceListener textListener = new TextWriterTraceListener(traceFilePath);
            CalcTraceSource.Listeners.Add(textListener);
            double mathResult;
            string output, startTime = null, endTime = null, result;
            try
            {
                //record start time of the test... 
                startTime = DateTime.Now.ToString();
                CalcTraceSource.TraceInformation("Starting test at time: {0}", startTime);

                //call the method to be tested and record the result... 
                mathResult = testMethod(num1, num2);

                //record end time of the test... 
                endTime = DateTime.Now.ToString();

                //write the test result... 

                result = mathResult == expectedResult ? "Passed" : "Failed";
                output = String.Format("{0},{1},Tested {2} {3} {4}); Expected {5}; Actual: {6};,,{7},{8},{9}",TestName,result,num1,operation,num2,expectedResult, mathResult, startTime,endTime, traceFilePath);
                Console.WriteLine(output);
                resultsFile.WriteLine(output);
                CalcTraceSource.TraceInformation("Output: " + output);
                CalcTraceSource.TraceInformation("Ending test at time: {0}", endTime);
            }
            catch (Exception e)
            {
                result = "Error";
                output = String.Format("{0},{1},Tested {2} {3} {4}); Expected {5}; Actual: {6};,,{7},{8},{9}", TestName, result, num1, operation, num2, expectedResult, e.Message, startTime, endTime, traceFilePath);
                output = output.Replace("\n", "\t"); // else, newlines from stack trace will mess with csv file... 
                output = output.Replace("\n", "\t"); // else, newlines from stack trace will mess with csv file... 
                Console.WriteLine(output);
                resultsFile.WriteLine(output);
                CalcTraceSource.TraceEvent(TraceEventType.Error, 2, "Test errored out...Output:\n" + output + "\n");
            }

            CalcTraceSource.Listeners.Remove(textListener);
            CalcTraceSource.Close();
        }
    }
}
