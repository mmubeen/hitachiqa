using BoDi;
using Gherkin;
using HitachiQA.Helpers;
using HitachiQA.Hooks.Browsers;
using HitachiQA.Playwright;
using HitachiQA.Source.HttpClients;
using HitachiQA.Source.HttpClients.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using TechTalk.SpecFlow;

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
