using HitachiQA.Playwright;
using Microsoft.Playwright;
using Reqnroll.BoDi;

namespace HitachiQA.UnitTests.StepDefinitions
{
    [Binding]
    public class PlaywrightStepDefinitions
    {
        private readonly IPage _playwrightPage;
        private readonly IBrowser _browser;
        private readonly IPlaywright _playwright;
        private readonly ScreenShot _screenShot;

        private readonly IObjectContainer _ioc;
        private BasePage Page { get; set; }

        public PlaywrightStepDefinitions(IPage p, IBrowser b, IPlaywright playwright, ScreenShot s)
        {
            _playwrightPage = p;
            _browser = b;
            _playwright = playwright;
            _screenShot = s;
        }
        [Given(@"Playwright is up")]
        public void GivenPlaywrightIsUp()
        {
            Page = new BasePage(_playwrightPage, _browser, _playwright, _screenShot);
        }

        [Then(@"user should land on HSAL homepage playwright")]
        public async Task ThenUserShouldLandOnHSALHomepagePlaywright()
        {
            await Page.Locator("xpath=//*[contains(text(), 'Hitachi')]").AssertIsPresentAsync();
            await Page.GetFieldAsync("open-global-search").ClickAsync();
            await Page.GetFieldAsync("site-search-keyword").SetFieldValueAsync("automation");
            await Page.Locator("xpath=//*[@aria-label='search']").ClickAsync();
            _screenShot.Error();
        }




    }
}
