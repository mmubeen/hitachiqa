using Reqnroll.BoDi;
using HitachiQA.Playwright;
using Microsoft.Playwright;

namespace {{ProjectName}}.Pages
{
    public class HsalHome : BasePage
    {
        public HsalHome(ObjectContainer OC) : base(OC)
        {
        }
        public string URL_PATH = "/";
        public async Task navigate() => await this.GotoAsync(URL_PATH);
        public ILocator OpenSearch => Locator("xpath=//*[@id='SupportNaviSearch']");
        public ILocator SearchInput => Locator("xpath=//*[@id='MF_form_phrase']");
        public ILocator SearchButton => Locator("xpath=//*[contains(@class, 'SearchBtn')]");
        public ILocator ResultSearchInput => Locator("xpath=//input[@title='search query']");

    }
}

