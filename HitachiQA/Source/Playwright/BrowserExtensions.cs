using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Playwright
{
    public static class BrowserExtensions
    {
        public async static Task<IPage> CreateNewPageAsync(this IBrowserContext context) {
           return await context.NewPageAsync();
        }
        public async static Task<IBrowserContext> CreateNewContextAsync(this IBrowser browser, string baseURL="")
        {
            return await browser.NewContextAsync(new() { RecordVideoDir = Path.Join(Directory.GetCurrentDirectory(),"/Videos/"), StrictSelectors = false, BaseURL=baseURL });

        }
    }
}
