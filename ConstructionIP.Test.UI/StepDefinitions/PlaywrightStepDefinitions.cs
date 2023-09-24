using HitachiQA.Dynamics.Playwright.CE;
using HitachiQA.Helpers;
using HitachiQA.Playwright;

namespace ConstructionIP.Test.UI.StepDefinitions
{
    [Binding]
    public class PlaywrightStepDefinitions
    {
        private AppLandingPage Page { get; }
        public PlaywrightStepDefinitions(AppLandingPage page)
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
            //await this.Page.GetAppButton("Dynamics 365 — custom").ClickAsync();
            await this.Page.GetLeftPaneSiteMapButton("Activities").ClickAsync();
            await this.Page.ClickCommandBarButtonAsync("Open Dashboards");
            await this.Page.ClickCommandBarButtonAsync("Refresh All");

        }

    }
}

