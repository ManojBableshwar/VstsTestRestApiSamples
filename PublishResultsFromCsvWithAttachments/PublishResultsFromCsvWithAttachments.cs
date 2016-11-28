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

namespace PublishResultsFromCsvWithAttachments
{
    class PublishResultsFromCsvWithAttachments
    {
        static void Main(string[] args)
        {

            string collectionUri;
            //set to Uri of the TFS collection
            //if this code is running in Build/Release workflow, we will fetch collection Uri from environment variable
            //See https://www.visualstudio.com/en-us/docs/build/define/variables for full list of agent environment variables
            if (Environment.GetEnvironmentVariable("TF_BUILD") == "True")
            {
                collectionUri = Environment.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI");
                Console.WriteLine("Fetched Collection (or VSTS account) from environment variable SYSTEM_TEAMFOUNDATIONCOLLECTIONURI: {0}", collectionUri);
            }
            else // set it to TFS instance of your choice 
            {
                collectionUri = "http://buildmachine1:8080/tfs/DefaultCollection";
                Console.WriteLine("Using Collection (or VSTS account): {0}", collectionUri);
            }

            //authentication.. 
            VssConnection connection = new VssConnection(new Uri(collectionUri), new VssCredentials());

            //set the team project name in which the test results must be published... 
            // get team project name from the agent environment variables if the script is running in Build/Release workflow..
            string teamProject;
            if (Environment.GetEnvironmentVariable("TF_BUILD") == "True")
            {
                teamProject = Environment.GetEnvironmentVariable("SYSTEM_TEAMPROJECT");
                Console.WriteLine("Fetched team project from environment variable SYSTEM_TEAMPROJECT: {0}", teamProject);
            }
            else //else set it the team project of your choice... 
            {
                teamProject = "FabrikamFiber";
                Console.WriteLine("Using team project: {0}", teamProject);
            }

            // get the build number to publish results against... 

            string buildNumber = null, buildUri = null, releaseUri = null, releaseEnvironmentUri = null; int buildId;
            // if this code is running in build/release workflow, we will use agent environment variables for fetch build/release Uris to associate information
            if (Environment.GetEnvironmentVariable("TF_BUILD") == "True")
            {
                //If RELEASE_RELEASEURI variable is set, then this code is running in the Release workflow, so we fetch release details 
                if (Environment.GetEnvironmentVariable("RELEASE_RELEASEURI") != "")
                {
                    releaseUri = Environment.GetEnvironmentVariable("RELEASE_RELEASEURI");
                    Console.WriteLine("Fetched release uri from environment variable RELEASE_RELEASEURI: {0}", releaseUri);

                    releaseEnvironmentUri = Environment.GetEnvironmentVariable("RELEASE_ENVIRONMENTURI");
                    Console.WriteLine("Fetched release environemnt uri from environment variable RELEASE_ENVIRONMENTURI: {0}", releaseEnvironmentUri);
                }
                //note that the build used to deploy and test a Release is an Aritfact.
                //If you have multiple builds or are using external artifacts like Jenkins, make sure you use Aritfact variables to find the build information
                //See https://www.visualstudio.com/en-us/docs/release/author-release-definition/understanding-tasks#predefvariables for pre-defined variables available in release
                //See https://www.visualstudio.com/en-us/docs/release/author-release-definition/understanding-artifacts#variables for artifact variables documentation
                //For example, you'll have to use RELEASE_ARTIFACTS_<aftifactname>_BUILDID to find the build number. 
                //Here we are assuming a simple setup, where we are working with Team Build and using Build variables instead... 

                //build number is human readable format of the build name, you can confiure it's format in build options..     
                buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
                Console.WriteLine("Fetched build number from environment variable BUILD_BUILDNUMBER: {0}", buildNumber);

                //build id is the id associated with the build number, which we will use to associate the test run with... 
                buildId = Convert.ToInt32(Environment.GetEnvironmentVariable("BUILD_BUILDID"));
                Console.WriteLine("Fetched build id from environment variable BUILD_BUILDID: {0}", buildId);

                //build uri is a more elaborate form of build id, in the vstfs:///Build/Build/<id> format... 
                //We will use this for querying test runs against this build...
                buildUri = Environment.GetEnvironmentVariable("BUILD_BUILDURI");
                Console.WriteLine("Fetched build uri from environment variable BUILD_BUILDURI: {0}", buildUri);
            }
            else //if the code is running in standalone mode, you'll have to use Build and Release APIs to fetch the build and release information... 
            //see https://www.visualstudio.com/en-us/docs/integrate/api/build/overview for build APIs... 
            //and https://www.visualstudio.com/en-us/docs/release/overview for release APIs... 
            {
                buildNumber = "20161124.2";
                buildId = 3;
                buildUri = "vstfs:///Build/Build/40";
                releaseUri = "vstfs:///ReleaseManagement/Release/2";
                releaseEnvironmentUri = "vstfs:///ReleaseManagement/Environment/2";
                Console.WriteLine("Using build number: {0}; build id: {1}; build uri: {2}; release uri: {3}; release environment uri: {4}", buildNumber, buildId, buildUri, releaseUri, releaseEnvironmentUri);
            }

            //Client to use test run and test result APIs... 
            TestManagementHttpClient client = connection.GetClient<TestManagementHttpClient>();

            //Test run model to initialize test run parameters..
            //For automated test runs, isAutomated must be set.. Else manual test run will be created..

            //<<Q: do we want to mark run in progress here?>>
            RunCreateModel TestRunModel = new RunCreateModel(name: "Sample test run from CSV file against buildNumber: " + buildNumber, isAutomated: true,
            startedDate: DateTime.Now.ToString(), buildId: buildId, releaseUri: releaseUri, releaseEnvironmentUri: releaseEnvironmentUri);

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


            TestAttachmentRequestModel fileAttachment = GetAttachmentRequestModel(resultsFilePath);
            TestAttachmentReference testAttachment =  client.CreateTestRunAttachmentAsync(fileAttachment, teamProject, testRun.Id).Result;
            Console.WriteLine("Created test run attachment -> Id: {0}, Url: {2}", testAttachment.Id, testAttachment.Url);

            //List to hold results from parsed from CSV file... 
            List<TestResultCreateModel> testResultsFromCsv = new List<TestResultCreateModel>();

            //TestMethodName, Outcome, ErrorMessage, StackTrace, StartedDate, CompletedDate
            string traceFilePath;
            List<string> resultLogFiles = new List<string>();  

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

                //store the path of the attachment in a list. We will publish attachments to results after we publish results and obtain result ids.
                traceFilePath = values[6]; resultLogFiles.Add(traceFilePath);

                //Add the result ot the results list... 
                testResultsFromCsv.Add(testResultModel);
            }

            //Publish the results... 
            List<TestCaseResult> publishedResults = client.AddTestResultsToTestRunAsync(testResultsFromCsv.ToArray(), teamProject, testRun.Id).Result;
            Console.WriteLine("Step 2: test results published...");

            //results are published in the order they were submitted. H
            //We will now loop through the results file and publish the attachments for each test result. 
            //Path of the attachment for each result (sample trace log) is in last column of the result csv file... 
            int i = 0;
            foreach (TestCaseResult r in publishedResults)
            {
                Console.WriteLine("Adding attachment for Test Result -> Id: {0}", r.Id); 
                fileAttachment = GetAttachmentRequestModel(resultLogFiles.ElementAt(i++));
                testAttachment = client.CreateTestResultAttachmentAsync(fileAttachment, teamProject, testRun.Id, r.Id).Result;
                Console.WriteLine("Created test result attachment -> Id: {0}, Url: {1}", testAttachment.Id, testAttachment.Url);
            }
            
            //Mark the run complete... 
            RunUpdateModel testRunUpdate = new RunUpdateModel(completedDate: DateTime.Now.ToString(), state: "Completed");
            TestRun RunUpdateResult = client.UpdateTestRunAsync(teamProject, testRun.Id, testRunUpdate).Result;

            Console.WriteLine("Step 3: Test run completed: {0}", RunUpdateResult.WebAccessUrl);

        }

        public static TestAttachmentRequestModel GetAttachmentRequestModel(string fileName)
        {
            byte[] bytes = File.ReadAllBytes(fileName);
            string encodedData = Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks);
            return new TestAttachmentRequestModel(encodedData, Path.GetFileName(fileName), "", AttachmentType.GeneralAttachment.ToString());
        }
    }
}
