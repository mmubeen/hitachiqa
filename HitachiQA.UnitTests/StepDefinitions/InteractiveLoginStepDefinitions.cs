using HitachiQA.Hooks.Browsers;
using HitachiQA.Source.Enums;
using HitachiQA.Source.Hooks.HttpClientExtras;
using HitachiQA.Source.HttpClients;
using HitachiQA.Source.HttpClients.Authorization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace HitachiQA.UnitTests.StepDefinitions;

[Binding]
public class InteractiveLoginStepDefinitions
{
    private readonly PlaywrightHook _playwrightHook;
    private readonly WebDriverHook _webDriverHook;


    private InteractivePlaywrightAuth PlaywrightInteractive { get; set; }
    private InteractiveWebdriverAuth WebDriverInteractive { get; set; }
    private AuthorizationClient AuthorizationClient { get; set; }
    private string BearerToken { get; set; }

    public InteractiveLoginStepDefinitions(
           PlaywrightHook playwrightHook,
           WebDriverHook webDriverHook)
    {
        _playwrightHook = playwrightHook;
        _webDriverHook = webDriverHook;
    }

    [Given(@"Configuration object is created for '([^']*)'")]
    public void GivenConfigurationObjectIsCreatedFor(FrameworkEnum framework)
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "HOST", "https://tfs-hisol-crm.visualstudio.com/" },
                { "FRAMEWORK", framework.ToString() },
                { "AUTH_INTERACTIVE_KEY_IDENTIFIER","8ca5dc71-c6d9-4a57-bda5-7135cfeea486.d10add66-4434-4bbf-bf55-21b049036d92" },
                { "AUTH_INTERACTIVE_EMAIL_IDENTIFIER", "@hitachisolutions.com"},
                { "AUTH_INTERACTIVE_PROFILE_NAME", "Default"},
                { "ENABLE_INTERACTIVE_AUTH", "true" }

            });

        var config = configBuilder.Build();

        PlaywrightInteractive = new(config, _playwrightHook);
        WebDriverInteractive = new(config, _webDriverHook);
        AuthorizationClient = new AuthorizationClient(PlaywrightInteractive, WebDriverInteractive, config);
    }

    [When(@"User gets a bearer token")]
    public async Task WhenUserGetsABearerToken()
    {
        BearerToken = await AuthorizationClient.GetBearerTokenAsync();
    }

    [Then(@"user should be able to successfully call the API")]
    public async Task ThenUserShouldBeAbleToSuccessfullyCallTheAPI()
    {
        var httpClient = new HttpClient(new HttpLoggingHandler());
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
        httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        var response = await httpClient.GetAsync("https://tfs-hisol-crm.visualstudio.com/_apis/projects?api-version=2.0");
        var data = await response.Content.ReadAsStringAsync();
        Log.Info(JToken.Parse(data));
    }
}
