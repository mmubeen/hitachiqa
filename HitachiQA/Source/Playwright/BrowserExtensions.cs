using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Source.Playwright
{
    public static class BrowserExtensions
    {
        public static IPage CreateNewPage(this IBrowserContext context) {
           return context.NewPageAsync().Result;
        }
        public static IBrowserContext CreateNewContext(this IBrowser browser)
        {
            return browser.NewContextAsync(new() { RecordVideoDir = Path.Join(Directory.GetCurrentDirectory(),"/Videos/"), StrictSelectors = false }).Result;

        }
    }
}
