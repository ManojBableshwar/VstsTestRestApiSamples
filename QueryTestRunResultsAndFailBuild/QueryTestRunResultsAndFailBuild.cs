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


namespace QueryTestRunResultsAndFailBuild
{
    class QueryTestRunResultsAndFailBuild
    {
        static void Main(string[] args)
        {


            string collectionUri;
            //set to Uri of the TFS collection
            //if this code is running in Build/Release workflow, we will fetch collection Uri from enviromnent variable
            //See https://www.visualstudio.com/en-us/docs/build/define/variables for full list of agent enviromnent variables
            if (Environment.GetEnvironmentVariable("TF_BUILD") == "True")
            {
                collectionUri = Environment.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI");
                Console.WriteLine("Fetched Collection (or VSTS account) from environment variable SYSTEM_TEAMFOUNDATIONCOLLECTIONURI: {0}", collectionUri);
            }
            else // set it to TFS instance of your choice 
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

            // get the build number to publis results against... 

            string buildNumber, buildUri; int buildId;
            // if this code is running in build workflow, BUILD_BUILDNUMBER and BUILD_BUILDID environment variables have the build info... 
            if (Environment.GetEnvironmentVariable("TF_BUILD") == "True")
            {
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

            //Query all test runs publishing against a build

            IList<TestRun> TestRunsAgainstBuild = client.GetTestRunsAsync(projectId: teamProject, buildUri: buildUri).Result;


            // if any of the test runs has tests that have not passed, then flag failure... 

            bool notAllTestsPassed = false;
            foreach (TestRun t in TestRunsAgainstBuild)
            {

                if (t.TotalTests != t.PassedTests)
                {
                    notAllTestsPassed = true;
                    Console.WriteLine("Test run: {0}; Total tests: {1}; Passed tests: {2} ", t.Name, t.TotalTests, t.PassedTests);
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
            } else
            {
                Console.WriteLine("All tests passed.. Returning success... ");
                Environment.Exit(0);
            }
        }
    }
}
