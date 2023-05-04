using HitachiQA.Helpers;
using HitachiQA.Source.Playwright;
using Microsoft.Playwright;
using System;
using TechTalk.SpecFlow;

namespace HitachiQA.UnitTests.StepDefinitions
{
    [Binding]
    public class PlaywrightStepDefinitions
    {
        private BasePage Page { get; }
        public PlaywrightStepDefinitions(BasePage page)
        {
            Page = page;
        }
        [Given(@"Playwright is up")]
        public void GivenPlaywrightIsUp()
        {
            Page.NullGuard();
        }

        [Then(@"user should land on HSAL homepage playwright")]
        public async Task ThenUserShouldLandOnHSALHomepagePlaywright()
        {
            await this.Page.Locator("xpath=//*[contains(text(), 'Hitachi')]").AssertIsPresentAsync();
            await this.Page.GetField("open-global-search").ClickAsync();
            await this.Page.GetField("site-search-keyword").SetFieldValueAsync("automation");
            await this.Page.Locator("xpath=//*[@role='submit']").ClickAsync();
        }

    }
}
