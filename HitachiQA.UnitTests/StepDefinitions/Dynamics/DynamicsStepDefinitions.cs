using HitachiQA.Dynamics.FS.Pages;
using HitachiQA.Helpers;
using Microsoft.Extensions.Configuration;

namespace HitachiQA.UnitTests.StepDefinitions.Dynamics
{
    [Binding]
    public class DynamicsStepDefinitions
    {
        private Dyn_LoginPage Page { get; }
        private IConfiguration Config;

        public DynamicsStepDefinitions(Dyn_LoginPage loginPage, IConfiguration Config)
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
            Page.UsernameTextField.SetFieldValue(Config["dynamics.username"]);

            //Next button
            Page.SubmitButton.Click();

            Page.PasswordTextField.SetFieldValue(Config["dynamics.password"]);

            //Sign in button
            Page.SubmitButton.Click();

            if (!string.IsNullOrWhiteSpace(Config.GetVariable("mfa.secret", true)))
            {
                Page.GetField("PhoneAppOTP").Click();

                var code = Functions.GenerateMFAOneTimeCode("2vrcg2yhplhzvxfz");
                Page.GetField("otc").SetFieldValue(code);
                Page.SubmitButton.Click();
            }
            else
            {
                Page.GetField("PhoneAppNotification").Click();
            }

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
