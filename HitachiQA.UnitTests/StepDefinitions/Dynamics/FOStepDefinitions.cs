using HitachiQA.Dynamics.FO.Pages;
using System;
using TechTalk.SpecFlow;

namespace HitachiQA.UnitTests.StepDefinitions.Dynamics
{
    [Binding]
    public class FOStepDefinitions
    {
        public Dyn_BasePage Page { get; }
        public FOStepDefinitions(Dyn_BasePage Page)
        {
            this.Page = Page;
        }

        [Given(@"user is in company '([^']*)' context")]
        public void GivenUserIsInCompanyContext(string companyName)
        {
            this.Page.GlobalCommandBar.SwitchCompanyContext(companyName);
        }

        [When(@"user clicks on '([^']*)' tile")]
        public void WhenUserClicksOnTile(string tileDisplayText)
        {
            this.Page.GetElementByText(tileDisplayText).Click();
        }

        [When(@"user clicks on '([^']*)'")]
        public void WhenUserClicksOn(string displayText)
        {
            this.Page.GetElementByText(displayText).Click();
        }
        [When(@"user fills out FO UI form")]
        public void WhenUserFillsOutUIForm(Table table)
        {
            foreach (var row in table.Rows)
            {
                var fieldName = row["fieldName"];
                var value = row["value"];

                this.Page.GetField(fieldName).SetFieldValue(value);
            }
        }


    }
}
