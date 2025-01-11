using HitachiQA.Playwright;
using Reqnroll.BoDi;

namespace HitachiQA.UnitTests.StepDefinitions
{
    [Binding]
    public class PlaywrightStepDefinitions
    {
        private readonly IObjectContainer _ioc;
        private BasePage Page { get; set; }

        public PlaywrightStepDefinitions(
            IObjectContainer ioc)
        {
            _ioc = ioc;
        }
        [Given(@"Playwright is up")]
        public void GivenPlaywrightIsUp()
        {
            Page = new BasePage(_ioc);
        }

        [Then(@"user should land on HSAL homepage playwright")]
        public async Task ThenUserShouldLandOnHSALHomepagePlaywright()
        {
            await Page.Locator("xpath=//*[contains(text(), 'Hitachi')]").AssertIsPresentAsync();
            await Page.GetFieldAsync("open-global-search").ClickAsync();
            await Page.GetFieldAsync("site-search-keyword").SetFieldValueAsync("automation");
            await Page.Locator("xpath=//*[@aria-label='search']").ClickAsync();
        }




    }
}
