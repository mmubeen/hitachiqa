using BoDi;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Playwright
{
    public class BasePage
    {
        public IPage PlaywrightPage { get; set; }
        public IBrowser Browser { get; set; }
        public IPlaywright Playwright { get; set; }
        public ScreenShot ScreenShot{ get; set;}

        public BasePage(IObjectContainer oc)
        {
            
            this.PlaywrightPage = oc.Resolve<IPage>();
            this.Browser= oc.Resolve<IBrowser>();
            this.Playwright = oc.Resolve<IPlaywright>();
            this.ScreenShot = oc.Resolve<ScreenShot>();
        }

        public ILocator GetField(string identifier)=> PlaywrightPage.GetField(identifier);

        public ILocator Locator(string selector, PageLocatorOptions? options = default) => PlaywrightPage.Locator(selector, options);
        
        public async Task<IResponse?> GotoAsync(string url, PageGotoOptions? options = null) => await PlaywrightPage.GotoAsync(url, options);

    }
}
