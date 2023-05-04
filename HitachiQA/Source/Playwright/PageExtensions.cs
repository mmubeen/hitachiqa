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
        public static ILocator GetField(this IPage page, string indentifier)
        {
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
            
            var completed = Task.WhenAny(tasks.Values).Result;
                        
            if (completed.Result== true)
            {
                var identifierFound = tasks.First(it => it.Value.IsCompletedSuccessfully).Key;
                ct.Cancel();
                return page.Locator(identifierFound);
            }
            throw new Exception($"Not found in UI: {indentifier}");


        }
        [DebuggerHidden]
        private static Task<bool> InvokeTrial(IPage page, ILocator fieldTrial, CancellationToken ct)
        {
            var retry = Policy.HandleResult<int>(0).WaitAndRetry(5 * 30, _ => TimeSpan.FromMilliseconds(200));
            
            var task = new Task<bool>(() => {

                var count =  retry.Execute(() =>
                {
                    if(ct.IsCancellationRequested)
                    {
                        return -1;
                    }
                    return fieldTrial.CountAsync().Result;
                });
                return count != 0;
                
            });
            task.Start();
            return task??throw new Exception("error invoking trial, task was null");
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
            "//a[@title='{input}']",
            "//button[@data-id='{input}']",
            "//*[@aria-label='{input}']",
            "//li[@title='{input}']",
            "//td[@data-hslcolumnname='{input}']",
            "//button[@id='{input}']",
            "//button[@title='{input}']",
            "//label[normalize-space(text())='{input}']/following-sibling::input",
            "//input[@title='{input}']"
        };
    }
}
