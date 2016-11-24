using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

// https://www.nuget.org/packages/Microsoft.TeamFoundationServer.Client/
using Microsoft.TeamFoundation.TestManagement.WebApi;

// https://www.nuget.org/packages/Microsoft.VisualStudio.Services.InteractiveClient/
using Microsoft.VisualStudio.Services.Client;

// https://www.nuget.org/packages/Microsoft.VisualStudio.Services.Client/
using Microsoft.VisualStudio.Services.Common;

using System.Diagnostics;

namespace PublishResultsFromCSV
{
    class PublishResultsFromCSV
    {
        static void Main(string[] args)
        {
            //set to Uri of the TFS collection
            //string collectionUri = "http://buildmachine1:8080/tfs/TestDefault";
            //since this scipt is running in Build/Release workflow, we will fetch collection Uri from enviromnent variable
            //See https://www.visualstudio.com/en-us/docs/build/define/variables for full list of agent enviromnent variables
            string collectionUri = Environment.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI");

            //set the team project name in which the test results must be published... 
            //string teamProject = "DefaultAgileGitProject";
            // get team project name from the agent environment variables
            string teamProject = Environment.GetEnvironmentVariable("SYSTEM_TEAMPROJECT");

            //authentication.. 
            VssConnection connection = new VssConnection(new Uri(collectionUri), new VssCredentials());

            //Client to use test run and test result APIs... 
            TestManagementHttpClient client = connection.GetClient<TestManagementHttpClient>();

            //Test run model to initialize test run parameters..
            //For automated test runs, isAutomated must be set.. Else manual test run will be created..

            //<<Q: do we want to mark run in progress here?>>
            RunCreateModel TestRunModel = new RunCreateModel(name: "Sample test run from CSV file", isAutomated: true, 
                startedDate: DateTime.Now.ToString());

            //Since we are doing a Asycn call, .Result will wait for the call to complete... 
            TestRun testRun = client.CreateTestRunAsync(teamProject, TestRunModel).Result;
            Console.WriteLine("Step 1: test run created -> " + testRun.Id + ": " + testRun.Name + " , Run url: " + testRun.WebAccessUrl);

            string resultsFilePath;
            if (args.Length == 0)
            {
                resultsFilePath = "Results.csv";
            }
            else
            {
                resultsFilePath = args[0];
            }

            //List to hold results from parsed from CSV file... 
            List<TestResultCreateModel> testResultsFromCsv = new List<TestResultCreateModel>();

            var reader = new StreamReader(File.OpenRead(resultsFilePath));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                Console.WriteLine("Publishing test {0}", values[0]);

                //Assign values from each line in CSV to result model... 
                TestResultCreateModel testResultModel = new TestResultCreateModel();
                testResultModel.TestCaseTitle = testResultModel.AutomatedTestName = values[0];
                testResultModel.Outcome = values[1];
                testResultModel.State = "Completed";
                testResultModel.ErrorMessage = values[2];
                testResultModel.StackTrace = values[3];
                testResultModel.StartedDate = values[4];
                testResultModel.CompletedDate = values[5];

                //Add the result ot the results list... 
                testResultsFromCsv.Add(testResultModel);
            }

            //Publish the results... 
            List<TestCaseResult> resultObj = client.AddTestResultsToTestRunAsync(testResultsFromCsv.ToArray(), teamProject, testRun.Id).Result;

            Console.WriteLine("Step 2: test results published: ");

            //Mark the run complete... 
            RunUpdateModel testRunUpdate = new RunUpdateModel(completedDate: DateTime.Now.ToString(), state: "Completed");
            TestRun RunUpdateResult = client.UpdateTestRunAsync(teamProject, testRun.Id, testRunUpdate).Result;

            Console.WriteLine("Step 3: Test run completed: " + RunUpdateResult.Url);

            //Console.ReadKey();
        }
    }
}
