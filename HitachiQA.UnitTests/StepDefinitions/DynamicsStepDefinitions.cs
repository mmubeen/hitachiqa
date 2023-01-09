using HitachiQA.Dynamics.Pages;
using HitachiQA.Source.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using TechTalk.SpecFlow;

namespace HitachiQA.UnitTests.StepDefinitions
{
    [Binding]
    public class DynamicsStepDefinitions
    {
        private Dynamics_LoginPage Page { get; }
        private IConfiguration Config;
        private SharedData SharedData { get; }

        public DynamicsStepDefinitions(Dynamics_LoginPage loginPage, IConfiguration Config, SharedData SD)
        {
            this.Page = loginPage;
            this.Config = Config;
            this.SharedData = SD;
        }

        [Given(@"user landed in dynamics login page")]
        public void GivenUserLandedInDynamicsLoginPage()
        {
            Log.Info(this.Page.GetCurrentURL());
        }

        [When(@"user signs in")]
        public void WhenUserSignsIn()
        {
            this.Page.UsernameTextField.setText(this.Config["dynamics.username"]);

            //Next button
            this.Page.SubmitButton.Click();

            this.Page.PasswordTextField.setText(this.Config["dynamics.password"]);

            //Sign in button
            this.Page.SubmitButton.Click();

            //Yes (stay signed in)
            if(this.Page.SubmitButton.GetAttribute("value")=="Sign in")
            {
                throw new Exception($"Sign In Failed, UI Message: {this.Page.Element("//*[@id='passwordError']").GetElementText()}");
            }
            this.Page.SubmitButton.Click();

        }

        [Then(@"user should be signed in")]
        public void ThenUserShouldBeSignedIn()
        {
            this.Page.Element("//*[text()=\"Dynamics 365\"]").assertElementIsVisible();
        }

        [Given(@"user is signed in")]
        public void GivenUserIsSignedIn()
        {
            WhenUserSignsIn();
            ThenUserShouldBeSignedIn();
        }
        private string AppTitle;
        [When(@"user navigates to '([^']*)' app")]
        public void WhenUserNavigatesToApp(string title)
        {
            this.Page.GetModuleCard(title).Click();
            this.SharedData.SetValue("Application", "CurrentApplication", title);

        }

        [Then(@"user should be in the previously navigated app")]
        public void ThenUserShouldBeInThePreviouslyNavigatedApp()
        {
            this.Page.AppBreadCrumb.GetAttribute("aria-label").Should().BeEquivalentTo(this.SharedData.GetValue("Application", "CurrentApplication"));
        }

        [When(@"user navigates to '([^']*)' page")]
        public void WhenUserNavigatesToPage(string pageName)
        {
            this.Page.GetLeftPaneSiteMapButton(pageName).Click();
            this.Page.Grid.OpenGridRecord("Customer Name", "lentil");


            this.Page.Element("//*[@data-id=\"name\"]").SetFieldValue("Miguel");
            this.Page.Element("//*[@data-id=\"telephone1\"]").SetFieldValue("201 790 0720");
            this.Page.Element("//*[@data-id=\"fax\"]").SetFieldValue("2017900720");
            this.Page.Element("//*[@data-id=\"websiteurl\"]").SetFieldValue("miguel.com");
            this.Page.Element("//*[@data-id=\"parentaccountid\"]").SetFieldValue("abc");

            //this.Page.GetEntityTab("Servicing").Click();
            //Log.Info(this.Page.GetGrid("WORK ORDERS").GetGridItems());

            //Log.Info(this.Page.Grid.GetGridItems());
        }



    }
}
