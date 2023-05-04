using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Playwright
{
    public static class PageExtensions
    {
        public static ILocator GetField(this IPage page, string indentifier)
        {
            var tasks = new Dictionary<string, Task<int>>();
            foreach (var trial in KnownFieldXPaths)
            {
                var identifier = $"xpath={trial.Replace("{input}", indentifier)} /self::*[not(contains(@style,'display: none'))]";
                tasks.Add(identifier, page.Locator(identifier).CountAsync());
            }
            var result = Task.WhenAll(tasks.Values).Result;

            if (result.Any(it => it > 0))
            {
                var identifier = tasks.First(it => it.Value.Result > 0).Key;
                return page.Locator(identifier);
            }
            throw new Exception($"Not found in UI: {indentifier}");


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
