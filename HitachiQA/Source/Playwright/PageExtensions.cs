using Microsoft.Playwright;
using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Playwright
{
    public static class PageExtensions
    {
        public async static Task<ILocator> GetFieldAsync(this IPage page, string indentifier)
        {
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            var tasks = new Dictionary<string, Task<bool>>();
            var ct = new CancellationTokenSource();
            //invoking all trials
            foreach (var trial in KnownFieldXPaths)
            {
                var identifier = $"xpath={trial.Replace("{input}", indentifier)} /self::*[not(contains(@style,'display: none'))]";
                var field = page.Locator(identifier);
                
                //adding running task
                tasks.Add(identifier, InvokeTrial(page, field, ct.Token));

            }
            
            var completed = await Task.WhenAny(tasks.Values);
                        
            if ((await completed)== true)
            {
                var identifierFound = tasks.First(it => it.Value.IsCompletedSuccessfully).Key;
                ct.Cancel();
                await Task.WhenAll(tasks.Values);
                return page.Locator(identifierFound);
            }
            throw new Exception($"Not found in UI: {indentifier}");


        }

        [DebuggerHidden]
        private async static Task<bool> InvokeTrial(IPage page, ILocator fieldTrial, CancellationToken ct)
        {
            var retry = Policy.HandleResult<int>(0).WaitAndRetryAsync(5 * 30, _ => TimeSpan.FromMilliseconds(200));
            
            
            var count = await retry.ExecuteAsync(async () =>
            {
                if(ct.IsCancellationRequested)
                {
                    return -1;
                }
                try
                {
                    return await fieldTrial.CountAsync();
                }
                catch(Exception)
                {
                    return 0;
                }
            });

            return count != 0;

        }

        public static List<string> KnownFieldXPaths = new List<string>()
        {
            "//label[text()='{input}']/..",
            "//button[normalize-space(text())='{input}']",
            "//*[@data-id='{input}']",
            "//*[@id='{input}']",
            "//button[.//*[normalize-space(text())='{input}']]",
            "//a[.//*[normalize-space(text())='{input}']]",
            "//a[normalize-space(text())='{input}']",
            "//button[@data-id='{input}']",
            "//*[@aria-label='{input}']",
            "//td[@data-hslcolumnname='{input}']",
            "//button[@id='{input}']",
            "//label[normalize-space(text())='{input}']/following-sibling::input",
            "//*[@data-value='{input}']",
            "//*[@name='{input}']",
            "//*[@title='{input}']",

        };
    }
}
