using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Collections.Generic;
using System.Linq;

namespace BulkDeleteTestCaseWorkItems
{
    class BulkDeleteTestCaseWorkItems
    {
        static void Main(string[] args)
        {
            //Pre-req: Create a query of test cases with Work item queries to be bulk deleted and then use this sample
            //Command line usage: 
            //BulkDeleteTestCaseWorkItems.exe <account or tfs server> <team project name> <query in quotes> <pat token>
            //Example: BulkDeleteTestCaseWorkItems.exe https://manojbableshwar.visualstudio.com HealthClinic "Shared Queries/Troubleshooting/P4 Test Cases" <pat token>

            Uri accountUri = new Uri(args[0]);
            string teamProjectName = args[1];
            string witTestCaseQuery = args[2];
            string personalAccessToken = args[3];  // See https://www.visualstudio.com/docs/integrate/get-started/authentication/pats                

            // Create a connection to the account - use this for pat auth (maninly VSTS)
            //VssConnection connection = new VssConnection(accountUri, new VssBasicCredential(string.Empty, personalAccessToken));

            // Create a connection to the account - use this TFS auth
            VssConnection connection = new VssConnection(accountUri, new VssCredentials());

            // Get an instance of the work item tracking client to query test case work items to be deleted
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            // Get an instance of the work item tracking client to delete test cases
            TestManagementHttpClient testClient = connection.GetClient<TestManagementHttpClient>();

            //Get the ID of the query specified...
            QueryHierarchyItem query = witClient.GetQueryAsync(teamProjectName, witTestCaseQuery).Result;

            //Query work item ids in the query...
            WorkItemQueryResult TestCaseIdsToDelete = witClient.QueryByIdAsync(query.Id).Result;

            if (TestCaseIdsToDelete.WorkItems.Count() > 0)
            {
                Console.WriteLine("Found {0} work items in query '{1}'.. Proceeding to delete...", TestCaseIdsToDelete.WorkItems.Count(), witTestCaseQuery);
            }
            else
            {
                Console.WriteLine("Found {0} work items returned in query '{1}'; Exiting... ", TestCaseIdsToDelete.WorkItems.Count(), witTestCaseQuery);
                return;
            }

            //Extract work item Ids to fetch work item details.. 
            int[] workItemIds = TestCaseIdsToDelete.WorkItems.Select<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemReference, int>(wif => { return wif.Id; }).ToArray();
            string[] fields = new[]
                    {
                        "System.Id",
                        "System.Title",
                        "System.WorkItemType"
                    };

            // Fetch work item details.. 
            IEnumerable<WorkItem> TestCasesToDelete = witClient.GetWorkItemsAsync(workItemIds, fields, TestCaseIdsToDelete.AsOf).Result;

            foreach (var testcase in TestCasesToDelete)
            {
                // Skip if work item type is not test case, since DeleteTestCaseAsync is only for test case work item delete... 
                if (testcase.Fields["System.WorkItemType"].ToString() != "Test Case")
                {
                    Console.WriteLine("Not a test case work item, skipping: {0} => {1}", testcase.Id, testcase.Fields["System.Title"]);
                }
                else
                {
                    try
                    {
                        //delete test case work item...
                        testClient.DeleteTestCaseAsync(teamProjectName, Convert.ToInt32(testcase.Id)).SyncResult();
                        Console.WriteLine("Deleted testcase: {0} => {1}", testcase.Id, testcase.Fields["System.Title"]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to delete testcase {0} => {1}. Error: {2}", testcase.Id, testcase.Fields["System.Title"], e.Message);
                    }
                }
            }
        }
    }
}