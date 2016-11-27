using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// https://www.nuget.org/packages/Microsoft.TeamFoundationServer.Client/
using Microsoft.TeamFoundation.TestManagement.WebApi;

// https://www.nuget.org/packages/Microsoft.VisualStudio.Services.InteractiveClient/
using Microsoft.VisualStudio.Services.Client;

// https://www.nuget.org/packages/Microsoft.VisualStudio.Services.Client/
using Microsoft.VisualStudio.Services.Common;

namespace QueryTestRunResultsAndFailRelease
{
    class QueryTestRunResultsAndFailRelease
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
            // get team project name from the agent environment variables if the script is running in Build workflow..
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

            // get the build number to publis results against... 

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

            //Query all test runs publishing against a release environmment... 
            //Ideally, we'd use the GetTestRunsAsync with release uri and release environment uri filters here, 
            //but GetTestRunsAsync does not support those filters yet...
            //Hence we will use GetTestRunsByQueryAsync... 

            QueryModel runQuery = new QueryModel("Select * from TestRun where releaseUri='" + releaseUri + "' and releaseEnvironmentUri='" + releaseEnvironmentUri + "'");
            IList <TestRun> TestRunsAgainstBuild = client.GetTestRunsByQueryAsync(runQuery,teamProject).Result;

            // if any of the test runs has tests that have not passed, then flag failure... 

            bool notAllTestsPassed = false;
            foreach (TestRun t in TestRunsAgainstBuild)
            {
                Console.WriteLine("Test run: {0}; Total tests: {1}; Passed tests: {2} ", t.Name, t.TotalTests, t.PassedTests);
                if (t.TotalTests != t.PassedTests)
                {
                    notAllTestsPassed = true;
                }
                //though we don't need to get test results, 
                //we are getting test results and printing tests that failed just to demo how to query results in a test run
                IList<TestCaseResult> TestResutsInRun = client.GetTestResultsAsync(project: teamProject, runId: t.Id).Result;
                foreach (TestCaseResult r in TestResutsInRun)
                {
                    Console.WriteLine("Test: {0}; Outcome {1}", r.TestCaseTitle, r.Outcome);
                }
            }

            if (notAllTestsPassed)
            {
                Console.WriteLine("Not all tests passed.. Returning failure... ");
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("All tests passed.. Returning success... ");
                Environment.Exit(0);
            }
        }
    }
}