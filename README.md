# Introduction 
 
## **Getting Started on implementing projects**

Example [Here](https://github.com/Hitachi-SolutionsQA/Demo)
1. Download `Visual Studio 2022`
2. Install `Specflow for visual Studio` extension
3. Add a `"specflow.json"` file with the following content:
	```
	{
		"stepAssemblies": [
				{
				"assembly": "HitachiQA"
				}
			]
	} 
	```
4. create and **select** a `.runsettings` (Eg. `"qa.runsettings"`) file which at a minimal should have all the required variables from the Known Variables [table](#known-variables) (environment specific)
5. (optional) create an `"appsettings.json"` file which by design should house settings that persist across environments
5. Add HitachiQA binaries to the solution (Example [Here](https://github.com/Hitachi-SolutionsQA/Demo/blob/main/Demo/Demo.csproj))
6. Follow either approach from [Running any implementing project](#running-any-implementing-project) (preferably Approach 2)
>**`Important!` Make sure to set every newly created file to copy to the output directory:** \*Right click file\*-> Properties->Copy to Output Directory=`"Copy Always"`

</br></br>

## **Running any implementing project**

* Given getting started steps were all followed correctly, running the binaries requires downloading them for which authentication is needed

> **Approach 1 (internal):** Not recommended for clients (useful until approach 2 is completed)
>1. Make sure user is authenticated in Tools->Options->Azure Service Authentication
>2. Make sure user has contributor access to the [Fuctional Testing](https://dev.azure.com/HitachiQA/Functional%20Testing) project in ADO
>3. Add the HitachiQA feed to Visual Studio, instructions [here](https://dev.azure.com/HitachiQA/Functional%20Testing/_artifacts/feed/HitachiQA/connect/visual%20studio) (Options > NuGet Package Manager > Package Sources)

> **Approach 2 (external):** In order to push binaries to other projects, we use the following [Pipeline](https://dev.azure.com/HitachiQA/Functional%20Testing/_build?definitionId=2)
>1. On the other project, create a Feed (preferably named `HitachiQA` or `HitachiTest`) if one already exist, it's name must be added to the pipeline's yml similar to step 2
>2. Edit HitachiQA.ExternalOrgPush pipeline here add the `Target Organization` URL (Eg. https://dev.azure.com/HitachiQA) found [Here](https://github.com/Hitachi-SolutionsQA/Hitachi-QA/blob/master/azure-pipelines-external-push.yml)
>3. [retrieve a PAT](https://learn.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticat)
>4. kick off the pipeline with the three arguments above
>5. Add the feed on step 1 to Visual Studio (Options > NuGet Package Manager > Package Sources, similar to Approach 1->Step 3)


</br></br>
</br></br>



# Known Variables

>**important!** adding `_VARNAME` to any variable name denotes that the value will be another variable name to be used instead, triggering logic to look for that especified value in the configuration, as another variable. (E.g. providing the variable `"SQL_CONNECTION_STRING_VARNAME"`=`"SQL_STR"`, will trigger the logic to look for a variable named `"SQL_STR"` which will technically replace `"SQL_CONNECTION_STRING`

| Variable Name     				| Description  										  															|
| :---        						|  :---												  															|
| HOST								| (required) URL for app under test for UI tests (Eg. https://www.hitachi.us)									|
| BROWSER		   					| (required) supported: `Chrome`, `Firefox`, `Edge`   															|
| 									| 																												|
| SERVER_HOST   					| (optional) URL required for for RestAPI communication			  												|
| API_TENANT_ID   					| (optional) if a `NoBrowser` tag found, this will be required for RestAPI authentication						|
| API_CLIENT_ID   					| (optional) if a `NoBrowser` tag found, this will be required for RestAPI authentication						|
| API_CLIENT_SECRET					| (optional) if a `NoBrowser` tag found, this will be required for RestAPI authentication						|
| API_USERNAME   					| (optional) if a `NoBrowser` tag found, this will be required for RestAPI authentication						|
| API_PASSWORD   					| (optional) if a `NoBrowser` tag found, this will be required for RestAPI authentication						|
| 									| 																												|
| KEYVAULT_URI						| (optional) KeyVault URI for the main keyvault				   	  												|
| APP_CONFIG_URI					| (optional) AppConfig URI for the main AppConfig			   	  												|
| AUT_APP_CONFIG_URI				| (optional) AppConfig URI of the application under test's App Config											|
| AUT_KEYVAULT_URI					| (optional) KeyVault URI of the application under test's Keyvault												|
| 									| 																												|
| COSMOS_URI   						| (optional) Cosmos Database URI 																				|
| COSMOS_API_KEY   					| (optional) Cosmos API Key (Required if COSMOS_URI is provided)												|
| COSMOS_DATABASE_NAME  			| (optional) Cosmos Database Name (Required if COSMOS_URI is provided)											|
| 									| 																												|
| SQL_CONNECTION_STRING 			| (optional) SQL SERVER CONNECTION STRING																		|


</br></br>
</br></br>

# More Info:
## **RunSettings:**

In order to run & setup different environment configuration, we use Runsettings. By design, one should have a different .runsettings per environment

### **Folder Overview:**

TODO

## **How to build a Test Case**

### Guidelines
* Every testcase is individual in scope, meaning all test cases start at Homepage(Dashboard)
* Test cases do not rely on a previous test case(the test cases do not run in order)

### **Sample TestCase**

```
Feature: View an Issued Policy with Left Navigation Stepper
	As an underwriter, 
	I want to be able to view an issued Policy 
	with the Left Navigation stepper.

Scenario:TC01 Verify Policy Left Nav Exists
	Given User is on Homepage
	When User navigates to policy ID 10562
	Then Verify sidetab is present
	| Key | Value                 |
	| a   | Business Information |
	| b   | Contacts              |
	| c   | UW Questions          |
	| d   | Additional Questions  |
	| e   | Drivers               |

```

In the above file all 3 statements come from different specflow objects. 


## *Basic BDD Guidelines*
All tests are written using the BDD Gherkin Language. Here are some basic guidelines:

### Title
* An explicit title.

### Narrative
Should usually already be present in the US
 A short introductory section with the following structure:
* **As a:** the person or role who will benefit from the feature;
* **I want:** the feature;
* **so that:** the benefit or value of the feature.

### Acceptance criteria
 A description of each specific scenario of the narrative with the following structure:

Scenario: A simple description of what the scenario is doing, usually the TC title
* **Given:** the initial context at the beginning of the scenario, in one or more clauses;
* **When:** the event that triggers the scenario;
* **Then:** the expected outcome, in one or more clauses.



