using Microsoft.Playwright;
using Polly;
using System.Diagnostics;

namespace HitachiQA.Playwright
{
    public static class PageExtensions
    {
        public async static Task<ILocator> GetFieldAsync(this IPage page, string indentifier, int waitSecconds=30)
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
                tasks.Add(identifier, InvokeTrial(page, field, ct.Token, waitSecconds));

            }

            var completed = await Task.WhenAny(tasks.Values);

            if ((await completed) == true)
            {
                var identifierFound = tasks.First(it => it.Value.IsCompletedSuccessfully).Key;
                ct.Cancel();
                await Task.WhenAll(tasks.Values);
                return page.Locator(identifierFound);
            }
            throw new Exception($"Not found in UI: {indentifier}");


        }

        [DebuggerHidden]
        private async static Task<bool> InvokeTrial(IPage page, ILocator fieldTrial, CancellationToken ct, int waitSecconds=30)
        {
            var retry = Policy.HandleResult<int>(0).WaitAndRetryAsync(5 * waitSecconds, _ => TimeSpan.FromMilliseconds(200));


            var count = await retry.ExecuteAsync(async () =>
            {
                if (ct.IsCancellationRequested)
                {
                    return -1;
                }
                try
                {
                    return await fieldTrial.CountAsync();
                }
                catch (Exception)
                {
                    return 0;
                }
            });

            return count != 0;

        }

        public static List<string> KnownFieldXPaths = new List<string>()
        {
            "//*[@data-id='{input}']",
            "//button[contains(@class, 'dropdown') and @id='{input}' and following-sibling::ul[.//button]]/..",
            "//button[@id='{input}']",
            "//*[@id='{input}']",
            "//label[text()='{input}']/preceding-sibling::input[@type='radio']",
            "//label[text()='{input}']/preceding-sibling::input[@type='checkbox']",
            "//label[text()='{input}']/..",
            "//button[normalize-space(text())='{input}']",
            "//button[.//*[normalize-space(text())='{input}']]",
            "//a[.//*[normalize-space(text())='{input}']]",
            "//a[normalize-space(text())='{input}']",
            "//button[@data-id='{input}']",
            "//*[@aria-label='{input}']",
            "//td[@data-hslcolumnname='{input}']",
            "//label[normalize-space(text())='{input}']/following-sibling::input",
            "//*[@data-value='{input}']",
            "//*[@name='{input}']",
            "//*[@title='{input}']",

        };
    }
}
