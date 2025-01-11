using HitachiQA.Helpers;
using HitachiQA.Hooks.Browsers;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using Polly;

namespace HitachiQA.Source.HttpClients.Authorization;

public class InteractiveWebdriverAuth : InteractiveAuthBase
{
    private readonly WebDriverHook _webDriverHook;
    public InteractiveWebdriverAuth(IConfiguration config, WebDriverHook webDriverHook) : base(config)
    {
        _webDriverHook = webDriverHook;
    }
    public override bool IsBrowserRunning => _webDriverHook.WebDriver != null;

    public override Task InvokeBrowserAsync(string? profile = null)
    {
        if (profile != null)
        {
            throw new NotImplementedException("profile sign in not implemented for selenium, use playwright");
        }
        _webDriverHook.InvokeNewSeleniumDriver("edge");
        return Task.CompletedTask;
    }

    public override Task<BrowserCredential> GetAccessTokenCredsAsync(string keyIdentifier)
    {
        var driver = _webDriverHook.WebDriver
            ?? throw new NullReferenceException("[GetAccessTokenCreds] WebDriverHook.WebDriver was null, driver is expectd at this point");
        var retry = Polly.Policy
          .HandleResult<object>(r => r == null)
          .WaitAndRetry(120, _ => TimeSpan.FromSeconds(1));

        var accessToken = retry.Execute(() =>
                        driver.ExecuteScript(@$"for (let i = 0; i < localStorage.length; i++) {{
                                  const key = localStorage.key(i);
                                  if (key.includes('accesstoken') && key.includes('{keyIdentifier}')) {{                              
                                    return localStorage.getItem(key);
                                  }}
                                }}

                                for (let i = 0; i < localStorage.length; i++) {{
                                  const key = localStorage.key(i);
                                  if (key.includes('accesstoken')) {{                              
                                    return localStorage.getItem(key);
                                  }}
                                }}

                                return null;")
        );

        var res = System.Text.Json.JsonSerializer.Deserialize<BrowserCredential>((string)accessToken)
            ?? throw new NotFoundException("Attempted to get bearer token for 2 minutes but was unsuccessful");
        return Task.FromResult(res);

    }
    public override Task AttemptAutoSigninAsync(string? emailIdentifierKey)
    {
        var driver = _webDriverHook.WebDriver
            ?? throw new NullReferenceException("[GetAccessTokenCreds] WebDriverHook.WebDriver was null, driver is expectd at this point");
        var retry = Polly.Policy
            .HandleResult(false)
            .WaitAndRetry(10, _ => TimeSpan.FromSeconds(1));

        retry.Execute(() =>
        {
            var idleTimeMilis = 1000;
            var isNetworkIdle = (bool)driver.ExecuteScript(
                 @"return performance.getEntriesByType('resource').map(x => x.startTime + x.duration).every(x => x < performance.now() - arguments[0])",
                    idleTimeMilis);
            return isNetworkIdle;
        });
        //if user is already authenticated, then the below clicks the first account with biberk.com email
        driver.ExecuteScript($"document.evaluate(\"//small[contains(text(),'{emailIdentifierKey}')]/../..\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE).singleNodeValue?.click()");

        return Task.CompletedTask;
    }
    public override Task DisposeAsync()
    {
        _webDriverHook.WebDriver?.Close();
        _webDriverHook.WebDriver = null;
        return Task.CompletedTask;
    }

    public async override Task NavigateToHostIfNeededAsync()
    {
        var host = Config.GetVariable("HOST");
        if (!_webDriverHook.WebDriver.Url.Contains(host))
        {
            await _webDriverHook.WebDriver.Navigate().GoToUrlAsync(host);
        }

    }

}
