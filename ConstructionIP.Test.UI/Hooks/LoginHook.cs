using HitachiQA.Dynamics.Playwright;
using HitachiQA.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace ConstructionIP.Test.UI.Hooks
{
    [Binding]
    public class LoginHook
    {
        private MicrosoftSignInPage Page { get; }
        public IConfiguration Config { get; }

        public LoginHook(MicrosoftSignInPage page, IConfiguration config)
        {
            Page = page;
            Config = config;
        }

        [BeforeScenario(Order = 200)]
        public async Task BeforeScenario()
        {
            await Page.UsernameTextField.SetFieldValueAsync(Config["dynamics.username"]);

            //Next button
            await Page.SubmitButton.ClickAsync();

            await Page.PasswordTextField.SetFieldValueAsync(Config["dynamics.password"]);

            //Sign in button
            await Page.SubmitButton.ClickAsync();

            if (!string.IsNullOrWhiteSpace(Config.GetVariable("mfa.secret", true)))
            {
                await Page.PlaywrightPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await Page.PlaywrightPage.WaitForLoadStateAsync(LoadState.NetworkIdle);
                if (await Page.Locator("#idDiv_SAOTCAS_Description").ExistsAsync())
                {
                    await Page.GetFieldAsync("idDiv_SAOTCS_HavingTrouble").ClickAsync();
                }
                await Page.GetFieldAsync("PhoneAppOTP").ClickAsync();

                var code = Functions.GenerateMFAOneTimeCode(Config.GetVariable("mfa.secret"));
                await Page.GetFieldAsync("otc").SetFieldValueAsync(code);
                await Page.SubmitButton.ClickAsync();
            }
            else
            {
                await Page.GetFieldAsync("PhoneAppNotification").ClickAsync();
            }

            await Page.Locator("#KmsiDescription").AssertIsVissibleAsync();
            //Yes (stay signed in)
            if ((await Page.SubmitButton.GetAttributeAsync("value")) == "Sign in")
            {
                throw new Exception($"Sign In Failed, UI Message: {await Page.GetFieldAsync("//*[@id='passwordError']").TextContentAsync()}");
            }
            await Page.SubmitButton.ClickAsync();

        }

        [BeforeScenario(Order = 201)]
        public async Task NavigateToApp()
        {
            await Page.PlaywrightPage.GotoAsync($"main.aspx?app={Config.GetVariable("dynamics.app")}");
        }

        

    }
}
