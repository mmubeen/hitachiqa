using HitachiQA.Driver;
using HitachiQA.Dynamics.Pages;
using HitachiQA.Helpers;
using HitachiQA.Source.Helpers;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using TechTalk.SpecFlow;

namespace HitachiQA.UnitTests.StepDefinitions.Dynamics
{
    [Binding]
    public class DynamicsStepDefinitions
    {
        private Dynamics_LoginPage Page { get; }
        private IConfiguration Config;

        public DynamicsStepDefinitions(Dynamics_LoginPage loginPage, IConfiguration Config)
        {
            Page = loginPage;
            this.Config = Config;
        }

        [Given(@"user landed in dynamics login page")]
        public void GivenUserLandedInDynamicsLoginPage()
        {
            Log.Info(Page.GetCurrentURL());
        }

        [When(@"user signs in")]
        public void WhenUserSignsIn()
        {
            Page.UsernameTextField.setText(Config["dynamics.username"]);

            //Next button
            Page.SubmitButton.Click();

            Page.PasswordTextField.setText(Config["dynamics.password"]);

            //Sign in button
            Page.SubmitButton.Click();

            //Yes (stay signed in)
            if (Page.SubmitButton.GetAttribute("value") == "Sign in")
            {
                throw new Exception($"Sign In Failed, UI Message: {Page.Element("//*[@id='passwordError']").GetElementText()}");
            }
            Page.SubmitButton.Click();

        }
        [Given(@"user is signed in to '([^']*)'")]
        public void GivenUserIsSignedInTo(string identifier)
        {
            WhenUserSignsIn();
            ThenUserShouldBeSignedInTo(identifier);
        }


        [Then(@"user should be signed in to '([^']*)'")]
        public void ThenUserShouldBeSignedInTo(string identifier)
        {
            Page.Element($"//*[text()=\"{identifier}\"]").assertElementIsPresent();
        }





    }
}
