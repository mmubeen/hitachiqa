using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Source.Playwright
{
    public class BasePage
    {
        public IPage PlaywrightPage { get; set; }
        public IBrowser Browser { get; set; }
        public IPlaywright Playwright { get; set; }
        public BasePage(IPlaywright playwright, IBrowser browser, IPage page)
        {
            this.PlaywrightPage = page;
            this.Browser= browser;
            this.Playwright = playwright;
        }

        public ILocator GetField(string identifier)=> PlaywrightPage.GetField(identifier);

        public ILocator Locator(string selector, PageLocatorOptions? options = default) => PlaywrightPage.Locator(selector, options);
        

    }
}
