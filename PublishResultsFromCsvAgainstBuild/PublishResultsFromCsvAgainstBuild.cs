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

namespace PublishResultsFromCsvAgainstBuild
{
    class PublishResultsFromCsvAgainstBuild
    {
        static void Main(string[] args)
        {

            string collectionUri;
            //set to Uri of the TFS collection
            //if this code is running in Build/Release workflow, we will fetch collection Uri from environment variable
            //See https://www.visualstudio.com/en-us/docs/build/define/variables for full list of agent environment variables
            if (Environment.GetEnvironmentVariable("TF_BUILD") == "True") {
                collectionUri = Environment.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI");
                Console.WriteLine("Fetched Collection (or VSTS account) from environment variable SYSTEM_TEAMFOUNDATIONCOLLECTIONURI: {0}", collectionUri);
            } else // set it to TFS instance of your choice 
            {
                collectionUri = "http://buildmachine1:8080/tfs/TestDefault";
                Console.WriteLine("Using Collection (or VSTS account): {0}", collectionUri);
            }

            //authentication.. 
            VssConnection connection = new VssConnection(new Uri(collectionUri), new VssCredentials());

            //set the team project name in which the test results must be published... 
            // get team project name from the agent environment variables if the script is running in Build workflow..
            string teamProject;
            if (Environment.GetEnvironmentVariable("TF_BUILD") == "True")
            {
                teamProject = Environment.GetEnvironmentVariable("SYSTEM_TEAMPROJECT");
                Console.WriteLine("Fetched team project from environment variable SYSTEM_TEAMPROJECT: {0}", teamProject);
            }
            else //else set it the team project of your choice... 
            {
                teamProject = "DefaultAgileGitProject";
                Console.WriteLine("Using team project: {0}", teamProject);
            }

            // get the build number to publish results against... 

            string buildNumber, buildUri; int buildId;
            // if this code is running in build workflow, BUILD_BUILDNUMBER and BUILD_BUILDID environment variables have the build info... 
            if (Environment.GetEnvironmentVariable("TF_BUILD") == "True")
            {
                //build number is human readable format of the build name, you can confiure it's format in build options..     
                buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
                Console.WriteLine("Fetched build number from environment variable BUILD_BUILDNUMBER: {0}", buildNumber);

                //build id is the id associated with the build number, which we will use to associate the test run with... 
                buildId = Convert.ToInt32( Environment.GetEnvironmentVariable("BUILD_BUILDID"));
                Console.WriteLine("Fetched build id from environment variable BUILD_BUILDID: {0}", buildId);

                //build uri is a more elaborate form of build id, in the vstfs:///Build/Build/<id> format... 
                //We will use this for querying test runs against this build...
                buildUri = Environment.GetEnvironmentVariable("BUILD_BUILDURI");
                Console.WriteLine("Fetched build uri from environment variable BUILD_BUILDURI: {0}", buildUri);
            }
            else //if the code is running in standalone mode, you'll have to use Build APIs to fetch the build number... 
            //see https://www.visualstudio.com/en-us/docs/integrate/api/build/overview for build APIs... 
            {
                buildNumber = "20161124.2";
                buildId = 40;
                buildUri = "vstfs:///Build/Build/40";
                Console.WriteLine("Using build number: {0}; build id: {1}; build uri: {2}", buildNumber, buildId, buildUri);
            }     

            //Client to use test run and test result APIs... 
            TestManagementHttpClient client = connection.GetClient<TestManagementHttpClient>();

            //Test run model to initialize test run parameters..
            //For automated test runs, isAutomated must be set.. Else manual test run will be created..

            //<<Q: do we want to mark run in progress here?>>
            RunCreateModel TestRunModel = new RunCreateModel(name: "Sample test run from CSV file against buildNumber: " + buildNumber, isAutomated: true,
                startedDate: DateTime.Now.ToString(), buildId: buildId);

            //Since we are doing a Asycn call, .Result will wait for the call to complete... 
            TestRun testRun = client.CreateTestRunAsync(teamProject, TestRunModel).Result;
            Console.WriteLine("Step 1: test run created -> {0}: {1}; Run url: {2} ", testRun.Id, testRun.Name, testRun.WebAccessUrl);

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
                //Setting state to completed since we are only publishing results. 
                //In advanced scenarios, you can choose to create a test result, 
                // move it into in progress state while test test acutally runs and finally update the outcome with state as completed
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

            Console.WriteLine("Step 2: test results published...");

            //Mark the run complete... 
            RunUpdateModel testRunUpdate = new RunUpdateModel(completedDate: DateTime.Now.ToString(), state: "Completed");
            TestRun RunUpdateResult = client.UpdateTestRunAsync(teamProject, testRun.Id, testRunUpdate).Result;

            Console.WriteLine("Step 3: Test run completed: {0}", RunUpdateResult.WebAccessUrl);

        }
    }
}
