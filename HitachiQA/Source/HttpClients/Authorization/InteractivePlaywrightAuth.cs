using HitachiQA.Hooks.Browsers;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using Polly;

namespace HitachiQA.Source.HttpClients.Authorization
{
    public class InteractivePlaywrightAuth : InteractiveAuthBase
    {
        private readonly PlaywrightHook _playwrightHook;

        public InteractivePlaywrightAuth(IConfiguration iconfig, PlaywrightHook playwrightHook) : base(iconfig)
        {
            _playwrightHook = playwrightHook;
        }

        public override bool IsBrowserRunning => _playwrightHook.PlaywrightPage != null;

        public async override Task AttemptAutoSigninAsync(string emailIdentifierKey)
        {
            var page = _playwrightHook.PlaywrightPage
             ?? throw new NullReferenceException("[GetAccessTokenCreds] WebDriverHook.WebDriver was null, driver is expectd at this point");
            var retry = Polly.Policy
                .HandleResult(false)
                .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(1));
            var idleTimeMilis = 2000;
            await retry.ExecuteAsync(async () =>
            {

                await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

                var isNetworkIdle = await page.EvaluateAsync<bool>(
                     @"idleTimeMilis=> performance.getEntriesByType('resource').map(x => x.startTime + x.duration).every(x => x < performance.now() - idleTimeMilis)",
                        idleTimeMilis);
                return isNetworkIdle;
            });
            //if user is already authenticated, then the below clicks the first account with biberk.com email
            var userXPath = $"//small[contains(text(),'{emailIdentifierKey}')]";
            if (await page.IsVisibleAsync(userXPath))
            {
                await page.ClickAsync(userXPath);
                await page.WaitForLoadStateAsync(LoadState.Load);
            }
        }



        public override async Task<BrowserCredential> GetAccessTokenCredsAsync(string identifierKey)
        {
            var browser = _playwrightHook.PlaywrightPage
            ?? throw new NullReferenceException("[GetAccessTokenCreds] PlaywrightHook.PlaywrightBrowser was null, driver is expectd at this point");
            var retry = Polly.Policy
              .HandleResult<object>(r => r == null)
              .WaitAndRetryAsync(60, _ => TimeSpan.FromSeconds(3));

            var host = Config.GetVariable("HOST");
            var server = new Uri(host).Host;
            await browser.WaitForURLAsync($"**/{server}/**", new() { Timeout = 120000 });
            // await browser.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(5000);
            var accessToken = await retry.ExecuteAsync(async () =>
            {
                var sessionRaw = await browser.EvaluateAsync("sessionStorage");
                var session = JObject.Parse(sessionRaw?.ToString());
                var localRaw = await browser.EvaluateAsync("localStorage");
                var local = JObject.Parse(localRaw?.ToString());
                var accessTokens = new JObject();

                foreach (var entry in session)
                {
                    if (entry.Key.Contains("accesstoken", StringComparison.InvariantCultureIgnoreCase))
                        accessTokens.Add(entry.Key, entry.Value);

                }

                foreach (var entry in local)
                {
                    if (entry.Key.Contains("accesstoken", StringComparison.InvariantCultureIgnoreCase))
                        accessTokens.Add(entry.Key, entry.Value);
                }

                foreach (var entry in accessTokens)
                {
                    if (entry.Key.Contains(identifierKey, StringComparison.InvariantCultureIgnoreCase))
                        return entry.Value.Value<string>();
                }

                if (accessTokens.Count == 0)
                    return null;

                return accessTokens.Properties().First().Value.Value<string>();
            }
            );


            return System.Text.Json.JsonSerializer.Deserialize<BrowserCredential>((string)accessToken)
                ?? throw new NotFoundException("Attempted to get bearer token for 2 minutes but was unsuccessful");
        }

        public override async Task InvokeBrowserAsync(string profile)
        {
            await _playwrightHook.InvokeBrowserAsync("msedge", Config.GetVariable("HOST"), profile);
        }

        public override async Task DisposeAsync()
        {
            await _playwrightHook.PlaywrightPage?.CloseAsync();
            _playwrightHook.PlaywrightPage = null;
        }

        public async override Task NavigateToHostIfNeededAsync()
        {
            var host = Config.GetVariable("HOST");
            if (!_playwrightHook.PlaywrightPage.Url.Contains(host))
            {
                await _playwrightHook.PlaywrightPage?.GotoAsync(host);
            }
        }
    }
}
