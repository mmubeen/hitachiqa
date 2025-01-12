using HitachiQA.Hooks.Browsers;
using HitachiQA.Playwright;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Reqnroll.BoDi;

namespace HitachiQA.UnitTests.StepDefinitions.Fields;


[Binding]
public class FunctionalFieldStepDefinitions
{

    private static IPage _page = null;
    private static async Task<IPage> GetPageAsync()
    {
        if(_page==null)
        {
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.AddJsonFile("appsettings.json").Build();
            var (browser, context) = await PlaywrightHook.InvokeNewPlaywrightBrowserAsync(config, playwright, "chrome");
            context = await browser.NewContextAsync(new() { RecordVideoDir = Path.Join(Directory.GetCurrentDirectory(), "/Videos/"), StrictSelectors = true});
            var page = await context.NewPageAsync();
            page.SetDefaultTimeout(5000);
            _page = page;
        }
        return _page;
    }

    private readonly TestContext _testContext;
    public FunctionalFieldStepDefinitions(PlaywrightHook playwrightHook, TestContext testContext)
    {
        _testContext = testContext;
    }

    [Given("The HTML page is loaded for {string}")]
    public async Task GivenTheHTMLPageIsLoadedFor(string pageName)
    {
        string filePath = $"./Data/Fields/FunctionalPages/{pageName}";
        filePath = filePath.EndsWith(".html")? filePath : $"{filePath}.html";
        var fullPath = Path.GetFullPath(filePath);
        var page = await GetPageAsync();
        //if(Path.GetFullPath(fullPath).Replace('\\', '/') != new Uri(page.Url).AbsolutePath)
        //{
        //    await page.GotoAsync(fullPath);
        //}
        var uri = new Uri(fullPath);
        Log.Info("uri: "+uri.ToString());
        await page.GotoAsync(uri.AbsoluteUri);


    }

    [When("User sets the value {string} for the field {string}")]
    public async Task WhenUserSetsTheValueForTheField(string value, string identifier)
    {
        var page = await GetPageAsync();
        await page.GetFieldAsync(identifier, 5).SetFieldValueAsync(value);
    }

    [When("User sets the value {string} for the field {string} as array")]
    public async Task WhenUserSetsTheValueForTheFieldAsArray(string value, string identifier)
    {
        var page = await GetPageAsync();
        var values = value.Split(';').Select(s=>s.Trim()).ToArray();
        await page.GetFieldAsync(identifier, 5).SetFieldValueAsync(values);
    }


    [Then("The field {string} value should be {string} or {string}")]
    public async Task ThenTheFieldValueShouldBeOr(string identifier, string expected, string enteredValue)
    {
        var page = await GetPageAsync();
        var value = await page.GetFieldAsync(identifier, 5).GetFieldValueAsync();
        if(!string.IsNullOrWhiteSpace(expected))
        {
            value.Should().Be(expected);
        }
        else
        {
            value.Should().Be(enteredValue);
        }

    }

    [When("User clicks radio button with label {string}")]
    public async Task WhenUserClicksRadioButtonWithLabel(string label)
    {
        var page = await GetPageAsync();
        await page.GetFieldAsync(label).ClickAsync();
    }

    [Then("Radio button with label {string} should be selected")]
    public void ThenRadioButtonWithLabelShouldBeSelected(string label)
    {
        throw new PendingStepException();
    }


    [Then("attach video")]
    public async Task ThenAttachVideo()
    {
        var page = await GetPageAsync();
        await PlaywrightHook.AttachVideoAsync(page, _testContext);
    }

}
