# Sample to bulk delete test case work items in TFS or VSTS

## Introduction

You can only delete single test case at a time in the TFS/VSTS Web UI. We have such a limitation because deleting test cases has downstream impact - all test points across test suites and test results associated with the test case are deleted. Unlike other work items like bugs, deleting test cases is irreversible. Meaning test cases cannot be recovered from ‘Recycle bin’. 

Having said that, there are genuine scenarios where bulk deletion of test case work items is needed. Examples include migration scenarios where a tool had incorrectly created test cases, etc. This is a tool to bulk delete test case work items. 

## Pre-requisites
Create a work item query of test cases that have to be bulk deleted and then use this sample against the query.

[Learn how to create PAT token](https://docs.microsoft.com/en-us/vsts/git/_shared/personal-access-tokens)  

[Learn how to create work item queries](https://docs.microsoft.com/en-us/vsts/work/track/using-queries)

## Command line usage
BulkDeleteTestCaseWorkItems.exe <account or tfs server including https or http> <team project name> <query containing test case work items, in quotes> <pat token>

## Sample output
```
BulkDeleteTestCaseWorkItems.exe https://manojbableshwar.visualstudio.com HealthClinic "Shared Queries/Troubleshooting/P4 Test Cases" <pat token>
Found 4 work items in query 'Shared Queries/Troubleshooting/P4 Test Cases'.. Proceeding to delete...
Deleted testcase: 4020 => hello world test case
Deleted testcase: 4021 => login test case
Deleted testcase: 4022 => logout test case
Deleted testcase: 4023 => add to cart test case
```

## Help
Email us at DevOps_Tools@microsoft.com for help... 