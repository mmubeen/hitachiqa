# Introduction 
 
## **Getting Started on implementing projects**

The objective is to create another project and then use HitachiQA as a nuget package. Example [Here](https://github.com/Hitachi-SolutionsQA/Demo)
1. Download `Visual Studio 2022`
2. Install `Specflow for visual Studio` extension
3. Create a new specflow project for *.NET6 using MSTest* 
4. download the following [specflow.json](README_FILES\specflow.json) file to the project folder
5. download the following [default.runsettings](README_FILES\default.runsettings) file to the project folder and modify it to your projects specifics 
	* It is recommend to rename it to the environment is pointing to like `qa.runsettings` 
	* at a minimal should have all the required variables from the Known Variables [table](#known-variables) (environment specific)
6. **select** a `.runsettings` (Eg. `"default.runsettings"`) file from step 5.
7. (optional) create an `"appsettings.json"` file which by design should house settings that persist across environments
8. Add HitachiQA reference to the project file (Example [Here](https://github.com/Hitachi-SolutionsQA/Demo/blob/main/Demo/Demo.csproj))
9. Follow either approach from [Running any implementing project](#running-any-implementing-project) (preferably Approach 2)
>**`Important!` Make sure to set every newly created file to copy to the output directory:** \*Right click file\*-> Properties->Copy to Output Directory=`"Copy Always"`

<br /><br />

## **Running any implementing project**

* Given getting started steps were all followed correctly, running the binaries requires downloading them for which authentication is needed

> **Approach 1 (internal):** Not recommended for clients (useful until approach 2 is completed)
>1. Make sure user is authenticated in Tools->Options->Azure Service Authentication
>2. Make sure user has contributor access to the [Fuctional Testing](https://dev.azure.com/HitachiQA/Functional%20Testing) project in ADO
>3. Add the HitachiQA feed to Visual Studio, instructions [here](https://dev.azure.com/HitachiQA/Functional%20Testing/_artifacts/feed/HitachiQA/connect/visual%20studio) (Options > NuGet Package Manager > Package Sources)

> **Approach 2 (external):** In order to push binaries to other projects, we use the following [Pipeline](https://dev.azure.com/HitachiQA/Functional%20Testing/_build?definitionId=4)
>1. On the other project, check if a feed already exists, if not create a Feed (preferably named `HitachiQA` or `HitachiTest`), it's url will be used in step 3.
>2. [retrieve a PAT](https://learn.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate)
>3. create a new Service Connection with the follwing naming convention ```<Project>_<your name>_ServiceConnection```, instructions [Here](https://learn.microsoft.com/en-us/azure/devops/pipelines/artifacts/nuget#:~:text=To%20publish%20a%20package%20to,Save%20when%20you%20are%20done.)
>4. Edit HitachiQA.ExternalOrgPush pipeline here add the newly created Service Connection name URL found [Here](https://dev.azure.com/HitachiQA/Functional%20Testing/_git/HitachiQA?path=/azure-pipelines-external-push.yml)
>5. kick off the pipeline with the three arguments above
>6. Add the feed on step 1 to Visual Studio (Options > NuGet Package Manager > Package Sources, similar to Approach 1->Step 3)


<br /><br />
<br /><br />



# Known Variables

>**important!** adding `_VARNAME` to any variable name denotes that the value will be another variable name to be used instead, triggering logic to look for that especified value in the configuration, as another variable. (E.g. providing the variable `"SQL_CONNECTION_STRING_VARNAME"`=`"SQL_STR"`, will trigger the logic to look for a variable named `"SQL_STR"` which will technically replace `"SQL_CONNECTION_STRING`

| Variable Name     				| Description  										  															|
| :---        						|  :---												  															|
| HOST								| (required) URL for app under test for UI tests (Eg. https://www.hitachi.us)									|
| BROWSER		   					| (required) supported: `Chrome`, `Firefox`, `Edge`   															|
| 									| 																												|
| OPTIONS      						| (optional) comma separated options to pass to the browser (E.g. --incognito;--no-sandbox)						|
| 									| 																												|
| DISABLE_AZURE_AUTHENTICATION      | (optional) If true, keyvault & appconfigs will be disabled even if the URI is provided  						|
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
| 									| 																												|
| SERVICE_BUS_NAMESPACE_URI			| (optional) Service Bus namespace URI 																			|


<br /><br />
<br /><br />

# UI Automation:
## Locators
* we recommend creating a Pages package in your project level to add each page (Eg. [here](https://github.com/Hitachi-SolutionsQA/Demo/blob/main/Demo/Pages/HsalHome.cs))
* We have created the HitachiQA.Driver.BasePage with the goal for it to be inherited in a BasePage of your own like this
	```
	//sample class
	public class HitachiBasePage : BasePage
	{
		public HitachiBasePage(ObjectContainer OC) : base(OC)
		{
		}
	}
	```
	>this will allow you to implement all functions that will be shared across every other page in your project. <br />
	>Here's an example of a very powerful function to get just about anyfield in dynamics using its logical name.
	```
	 public Element GetField(string fieldLogicalName) => Element("//*[@data-id='customer_name']")
	```
	>Please note: ideally, every other class in pages should inherit HitachiBasePage
## Actions

* As we attempt to innovate, we ambition any value setting be done through a single function called SetFieldValue
  HitachiQA.Driver.Element.SetFieldValue(string value) should work for setting any field value (Textfield, Dropdown, Checkbox)

	```
	//sample step definition using the above class & SetFieldValue
	public class HomePageStepDefinitions{

        private HitachiBasePage HitachiBasePage { get; }

        public HomePageStepDefinitions(HitachiBasePage hbp){
            this.HitachiBasePage = hbp;
        }

		[When(@"user sets '([^']*)' field value to '([^']*)'")]
        public void WhenUserSetsFieldValueTo(string fieldLogicalName, string value)
        {
            this.HitachiBasePage.GetField(fieldLogicalName).SetFieldValue(value);
        }

	}
	```

	>The way it works is by getting the inner html of the provided field and trying all the known xpaths in HitachiQA.Driver.UserActions.KnownXPaths. <br />
	>Once it finds a match then it looks on the corresponding entry value which is handled by a switch having the implementation for any type of field. <br />
	>if you need to add a known xpath we recommend adding it directly to the KnownXPaths dictionary along with unit tests for it but here's a quick why of doing it in implementing projects.
	```
	//add the following hook(hook) in a class preferably in a hooks folder
	[BeforeTestRun]
	public static void AddingXPathsHook()
	{
		HitachiQA.Driver.UserActions.KnownXPaths.Add("//input[@type='text' and contains(@data-id, 'text-input')]", "textfield")
	}
	```
## IFrames implicit handling!
* we recommend creating a class in the Pages package from Locators for each iframe to be handled<br />
  then load the IFrame by providing any of these property values 
  - IFrame (By)
  - IFrameTitle (string)
  - IFrameId (string)
	```
	//initializing a class with an IFrame
	public class AppLandingPage : HitachiBasePage
	{
		public AppLandingPage(ObjectContainer OC) : base(OC){
			this.IFrame = By.XPath("//*[@id='AppLanding']")
		}
        public Element GetModuleCard(string title) => Element($"//*[@title='{title}']");
	}
	```
	step definition:
	```
	[When(@"user clicks on '([^']*)' Module Card")]
	public void WhenUserSetsFieldValueTo(string title)
	{
		this.AppLandingPage.GetField("searchField").SetFieldValue(title)
		this.AppLandingPage.GetModuleCard(title).Click();
	}
	```
	**Note: both actions, SetFieldValue and Click will switch to the AppLanding IFrame before making the relevant action**

	>The way it works is that every element can possibly have a relevant IFrame and there is <br />
	>embeded functionality that attaches the loaded IFrame into every object initialized as part of a given object
	
	
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



