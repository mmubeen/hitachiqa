using Microsoft.Playwright;

namespace HitachiQA.Playwright
{
    public static class BrowserExtensions
    {
        public async static Task<IPage> CreateNewPageAsync(this IBrowserContext context)
        {
            return await context.NewPageAsync();
        }
        public async static Task<IBrowserContext> CreateNewContextAsync(this IBrowser browser, string baseURL = "")
        {
            return await browser.NewContextAsync(new() { RecordVideoDir = Path.Join(Directory.GetCurrentDirectory(), "/Videos/"), StrictSelectors = false, BaseURL = baseURL });

        }
    }
}
