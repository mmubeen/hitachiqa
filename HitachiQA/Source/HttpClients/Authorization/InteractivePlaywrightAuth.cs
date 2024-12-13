using HitachiQA.Helpers;
using HitachiQA.Hooks.Browsers;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using Polly;
using System.Diagnostics;

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

        public async override Task AttemptAutoSigninAsync(string? emailIdentifierKey)
        {
            var page = _playwrightHook.PlaywrightPage
             ?? throw new NullReferenceException("[GetAccessTokenCreds] WebDriverHook.WebDriver was null, driver is expectd at this point");
            var retry = Polly.Policy
                .HandleResult(false)
                .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(1));

            await retry.ExecuteAsync(async () => {
                var idleTimeMilis = 1000;
                await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

                var isNetworkIdle = await page.EvaluateAsync<bool>(
                     @"return performance.getEntriesByType('resource').map(x => x.startTime + x.duration).every(x => x < performance.now() - arguments[0])",
                        idleTimeMilis);
                return isNetworkIdle;
            });
            //if user is already authenticated, then the below clicks the first account with biberk.com email
            await page.EvaluateAsync($"document.evaluate(\"//small[contains(text(),'{emailIdentifierKey}')]/../..\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE).singleNodeValue?.click()");
        }

     

        public override async Task<BrowserCredential> GetAccessTokenCredsAsync(string identifierKey)
        {
            var browser = _playwrightHook.PlaywrightPage
            ?? throw new NullReferenceException("[GetAccessTokenCreds] PlaywrightHook.PlaywrightBrowser was null, driver is expectd at this point");
            var retry = Polly.Policy
              .HandleResult<object?>(r => r == null)
              .OrInner<PlaywrightException>()
              .WaitAndRetryAsync(120, _ => TimeSpan.FromSeconds(2));


            var accessToken = await retry.ExecuteAsync(async () => {
                var data = await _playwrightHook.PlaywrightBrowserContext.StorageStateAsync();
                var obj = JObject.Parse(data);
                var origins = obj["origins"];
                var accessTokens = new JArray();
                foreach (var origin in origins)
                {
                    var localStorage = origin.Value<JArray>("localStorage");
                    var accessTokensForOrigin = localStorage.Where(it => it.Value<string>("name").Contains("accesstoken"));
                    accessTokens = accessTokens.Concat(accessTokensForOrigin).ToJArray();
                }

                var matchingToken = accessTokens.FirstOrDefault(it => it.Value<string>("name").Contains(identifierKey));
                matchingToken ??= accessTokens.FirstOrDefault();
                return matchingToken?.Value<string?>("value");
            }
            );
            

            return System.Text.Json.JsonSerializer.Deserialize<BrowserCredential>((string)accessToken)
                ?? throw new NotFoundException("Attempted to get bearer token for 2 minutes but was unsuccessful"); 
        }

        public override async Task InvokeBrowserAsync()
        {
            await _playwrightHook.InvokeBrowserAsync("msedge");
        }

        public override async Task DisposeAsync()
        {
            await _playwrightHook.PlaywrightPage?.CloseAsync();
            _playwrightHook.PlaywrightPage = null;
        }

        public async override Task NavigateToHostIfNeededAsync()
        {
            var host = Config.GetVariable("HOST");
            if(!_playwrightHook.PlaywrightPage.Url.Contains(host))
            {
                await _playwrightHook.PlaywrightPage?.GotoAsync(host);
            }
        }
    }
}
