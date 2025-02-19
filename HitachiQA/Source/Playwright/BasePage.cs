using HitachiQA.Hooks.Browsers;
using Microsoft.Playwright;
using Reqnroll.BoDi;

namespace HitachiQA.Playwright
{
    public class BasePage
    {
        public IPage PlaywrightPage { get; set; }
        public IBrowser Browser { get; set; }
        public IPlaywright Playwright { get; set; }
        public ScreenShot ScreenShot { get; set; }

        public BasePage(PlaywrightHook hook)
        {
            PlaywrightPage = hook.PlaywrightPage;
            Browser = hook.PlaywrightBrowser;
            Playwright = hook.Playwright;
            
        }
        public BasePage(IPage page, IBrowser browser, IPlaywright playwright, ScreenShot screenShot=null)
        {
            PlaywrightPage = page;
            Browser = browser;
            Playwright = playwright;
            ScreenShot = screenShot;
        }
        public BasePage(IObjectContainer oc)
        {

            this.PlaywrightPage = oc.Resolve<IPage>();
            this.Browser = oc.Resolve<IBrowser>();
            this.Playwright = oc.Resolve<IPlaywright>();
            this.ScreenShot = oc.Resolve<ScreenShot>();
        }

        public async Task<ILocator> GetFieldAsync(string identifier) => await PlaywrightPage.GetFieldAsync(identifier);

        public ILocator Locator(string selector, PageLocatorOptions options = default) => PlaywrightPage.Locator(selector, options);

        public async Task<IResponse> GotoAsync(string url, PageGotoOptions options = null) => await PlaywrightPage.GotoAsync(url, options);

        public IFrameLocator FrameLocator(string selector) => PlaywrightPage.FrameLocator(selector);

    }
}
