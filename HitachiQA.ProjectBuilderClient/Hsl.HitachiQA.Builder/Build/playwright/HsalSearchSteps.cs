using {{ProjectName}}.Pages;
using HitachiQA;
using HitachiQA.Playwright;

namespace {{ProjectName}}.StepDefinition
{
    [Binding]
    public sealed class HsalSearchSteps
    {
        // For additional details on Reqnroll step definitions see https://docs.reqnroll.net/latest/automation/step-definitions.html

        public HsalHome HsalHome;
        public string searchCriteria;

        public HsalSearchSteps(HsalHome page)
        {
            this.HsalHome = page;
        }
        
        [Given(@"user landed on HSAL homepage")]
        public async Task GivenUserLandedOnHSALHomepage()
        {
            await this.HsalHome.navigate();
        }

        [When(@"user opens Search modal")]
        public async Task WhenUserOpensSearchModal()
        {
            await HsalHome.OpenSearch.ClickAsync();
        }

        [When(@"user types '([^']*)' in searchbox")]
        public async Task WhenUserTypesInSearchbox(string searchCriteria)
        {
            await HsalHome.SearchInput.SetFieldValueAsync(searchCriteria);
            this.searchCriteria = searchCriteria;
        }

        [When(@"user clicks on Search button")]
        public async Task WhenUserClicksOnSearchButton()
        {
            await HsalHome.SearchButton.ClickAsync();
        }

        [Then(@"user should be presented with search results from HSAL")]
        public async Task ThenUserShouldBePresentedWithSearchResultsFromHSAL()
        {
            var result = await HsalHome.ResultSearchInput.First.InputValueAsync();
            result.Should().Be(this.searchCriteria);
            Log.Info("This is an infomrmaitonal Message");
            HsalHome.ScreenShot.Info();
        }

    }
}
