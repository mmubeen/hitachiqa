using HitachiQA.Driver;
using HitachiQA.Hooks.Browsers;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using Reqnroll.BoDi;

namespace HitachiQA.UnitTests.StepDefinitions
{
    [Binding]
    public class ConfigurationStepDefinitions
    {
        string knownName = "THIS_IS_A_TEST";
        string varName = "THIS_IS_THE_TEST_VAR";
        string varVal = "VAR_VALUE";
        string? result = "";
        IConfiguration Config { get; set; }
        ObjectContainer ObjectContainer { get; }
        public ConfigurationStepDefinitions(ObjectContainer OC)
        {
            this.ObjectContainer = OC;
        }

        [Given(@"a test environment name is loaded")]
        public void GivenATestEnvironmentNameIsLoaded()
        {
            //set an environment variable with the <known name>_VARNAME format adn actual variable name as value
            //this is also the pair, scripters will provide in their solution
            Environment.SetEnvironmentVariable(knownName + "_VARNAME", varName);
            // set the actual variable with the desired value
            Environment.SetEnvironmentVariable(varName, varVal);
            this.Config = Main.BuildConfig();

        }


        [When(@"an environment variable is acquired with varname")]
        public void WhenAnEnvironmentVariableIsAcquiredWithVarname()
        {
            //given the known name for the variable
            this.result = this.Config.GetVariable(knownName, false);

        }

        [Then(@"the system should look for the varname provided")]
        public void ThenTheSystemShouldLookForTheVarnameProvided()
        {
            //the system should return the deried value set above
            this.result.Should().Be(this.varVal);
        }
        [Then(@"Webdriver related classes should not be initialized")]
        public void ThenWebdriverRelatedClassesShouldNotBeInitialized()
        {
            this.ObjectContainer.IsRegistered<UserActions>().Should().BeFalse();
            this.ObjectContainer.IsRegistered<IWebDriver>().Should().BeFalse();
        }

        [Then(@"NoBrowser tag on this feature should cascade down to this test")]
        public void ThenNoBrowserTagOnThisFeatureShouldCascadeDownToThisTest()
        {
            this.ObjectContainer.Resolve<BrowserIndicator>().IsBrowserFeature.Should().BeFalse();
        }

    }
}
