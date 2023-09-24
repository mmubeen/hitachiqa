using BoDi;
using HitachiQA.Playwright;
using Microsoft.Playwright;
using Polly;

namespace HitachiQA.Dynamics.Playwright
{
    public class Dyn_BasePage:BasePage
    {
        public Dyn_BasePage(IObjectContainer oc) : base(oc)
        {

        }
        public ILocator ToastNotificationCloseButton => Locator($"//*[@data-pa-dialog-popup]//*[@alt='close']");

        public ILocator GetLeftPaneSiteMapButton(string title) => Locator($"//*[@data-id='navbar-container'] //li[@aria-label='{title}' and contains(@id, 'sitemap-entity')]");

        private const string COMMAND_BAR_XPATH = "((//div[@id='mainContent'] //*[contains(@data-id, 'Command')])[1] | //*[@data-id='OverflowFlyout'])";

        public ILocator GetCommandBarButton(string displayText) => Locator($"{COMMAND_BAR_XPATH} //button[*//text()='{displayText}']");

        public async Task ClickCommandBarButtonAsync(string displayText)
        {
            if (await ToastNotificationCloseButton.ExistsAsync())
            {
                await ToastNotificationCloseButton.ClickAsync();
            }
            var showMore = Locator("(//div[@id='mainContent'] //*[contains(@data-id, 'Command')])[1]//button[@data-id='OverflowButton']/..");
            var retry = Policy
            .HandleResult<bool>(false)
            .WaitAndRetryAsync(new[]
                {
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(3),
                }
            );

            await Locator(COMMAND_BAR_XPATH).AssertIsPresentAsync();
            var targetCommand = GetCommandBarButton(displayText);

            await retry.ExecuteAsync(async () =>
            {
                if (await targetCommand.ExistsAsync())
                    return true;

                await showMore.ClickAsync();
                if (await targetCommand.ExistsAsync())
                    return true;

                await showMore.ClickAsync();
                return false;
            });

            await targetCommand.ClickAsync();

        }

    }

    public class MicrosoftSignInPage : BasePage
    {
        public MicrosoftSignInPage(IObjectContainer oc) : base(oc) {
            
        }
        public ILocator UsernameTextField => Locator("xpath=//input[@name='loginfmt']");
        public ILocator PasswordTextField => Locator("xpath=//input[@name='passwd']");
        public ILocator SubmitButton => Locator("xpath=//input[@type='submit']");
    }
}
