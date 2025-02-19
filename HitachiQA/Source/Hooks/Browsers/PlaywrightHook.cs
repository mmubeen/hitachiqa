using DocumentFormat.OpenXml.Drawing.Charts;
using HitachiQA.Helpers;
using HitachiQA.Playwright;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Reqnroll.BoDi;
using System.Runtime.InteropServices;

namespace HitachiQA.Hooks.Browsers
{
    [Binding]
    public class PlaywrightHook : HookBase
    {
        public static List<string> optionsList;

        public BrowserIndicator BrowserIndicator { get; init; }
        public IPlaywright Playwright { get; set; }
        public IPage PlaywrightPage { get; set; }
        public IBrowser PlaywrightBrowser { get; set; }
        public IBrowserContext PlaywrightBrowserContext { get; set; }
        public PlaywrightHook(IConfiguration config, BrowserIndicator bi) : base(config)
        {
            BrowserIndicator = bi;
        }

        public async Task InvokeBrowserAsync(string browserName, string host, string profile = null)
        {
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            if (profile != null)
            {
                (PlaywrightBrowser, PlaywrightBrowserContext) = await InvokeNewPlaywrightBrowserAsync(Configuration, Playwright, browserName, profile);
            }
            else
            {
                (PlaywrightBrowser,_) = await InvokeNewPlaywrightBrowserAsync(Configuration, Playwright, browserName);
                PlaywrightBrowserContext = await PlaywrightBrowser.CreateNewContextAsync(host);

            }
            PlaywrightBrowserContext.NullGuard();
            PlaywrightPage = await PlaywrightBrowserContext.CreateNewPageAsync();
        }


        [BeforeTestRun]
        public static void InstallPlaywright()
        {
            var framework = Main.Configuration.GetVariable("FRAMEWORK", true);
            var installPlaywright = Main.Configuration.GetVariable("INSTALL_PLAYWRIGHT", true);
            if (installPlaywright?.ToUpper() == "TRUE" || framework?.ToUpper() == "PLAYWRIGHT")
            {
                Microsoft.Playwright.Program.Main(["install", "--with-deps"]);
            }
        }

        [BeforeScenario(Order = 2)]
        public async Task InvokeBrowserAsync(FeatureContext fc, ScenarioContext sc, IObjectContainer oc, TestContext tc)
        {
            if (!fc.FeatureInfo.Tags.Contains("NoBrowser") && !sc.ScenarioInfo.Tags.Contains("NoBrowser"))
            {

                BrowserIndicator.IsBrowserFeature = true;
                //if no selenium tag
                //and either driver is playwright or feature tag contains playwright
                //           
                if (ShouldUsePlaywright(fc, sc, Configuration))
                {
                    var browser = Configuration.GetVariable("BROWSER");
                    var host = Configuration.GetVariable("HOST");
                    await InvokeBrowserAsync(browser, host);
                    oc.RegisterInstanceAs<IPlaywright>(Playwright);
                    oc.RegisterInstanceAs<IBrowser>(PlaywrightBrowser);
                    oc.RegisterInstanceAs<IBrowserContext>(PlaywrightBrowserContext);
                    oc.RegisterInstanceAs<IPage>(PlaywrightPage);
                    oc.RegisterInstanceAs<ScreenShot>(new ScreenShot(fc, sc, PlaywrightPage, tc));
                    await PlaywrightPage.GotoAsync("/");
                }
            }

        }

        [AfterScenario]
        public async Task closeContext(TestContext tc)
        {
            if (this.PlaywrightPage != null && PlaywrightPage.Video != null)
            {
                await AttachVideoAsync(PlaywrightPage, tc);
                await PlaywrightBrowserContext?.CloseAsync();
            }
        }

        public static async Task AttachVideoAsync(IPage page, TestContext msContext)
        {
            var videoPath = await page.Video.PathAsync();
            msContext.AddResultFile(Path.GetFullPath(videoPath));
            Console.WriteLine($"\nVideo: {new Uri(videoPath)}\n");
        }

        public static async Task<(IBrowser, IBrowserContext)> InvokeNewPlaywrightBrowserAsync(IConfiguration config, IPlaywright playwright, string browserName, string profile = null)
        {
            if (playwright == null)
                throw new Exception("Playwright must be initialized before invoking browser");

            IBrowser browser;
            IBrowserContext context=null;
            switch (browserName.ToLower())
            {
                case "chrome":
                    browser = await playwright.Chromium.LaunchAsync(GetPlaywrightOptions(config, "chrome"));
                    break;
                case "msedge":
                    if (profile != null)
                    {
                        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            throw new NotImplementedException("InvokeNewPlaywrightBrowserAsync with profile only works on windows");
                        }
                        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        var userData = Path.Combine(userProfile, "AppData\\Local\\Microsoft\\Edge\\User%20Data\\");
                        context = await playwright
                            .Chromium
                            .LaunchPersistentContextAsync(userData, GetPlaywrightOptionsWithProfile(config, profile, "msedge"));
                        return (context.Browser, context);
                    }
                    else
                    {
                        browser = await playwright.Chromium.LaunchAsync(GetPlaywrightOptions(config, "msedge"));
                    }
                    break;
                default:
                    if (string.IsNullOrWhiteSpace(browserName))
                    {
                        throw new InvalidOperationException("BROWSER variable was not set, most likely forgot to select a .runsettings file. Refer to README for more info");
                    }
                    throw new NotImplementedException($"BROWSER value={browserName} is not supported");
            }

            return (browser, context);


        }

        private static bool ShouldUsePlaywright(FeatureContext fc, ScenarioContext sc, IConfiguration config)
        {
            var tags = fc.FeatureInfo.Tags.Concat(sc.ScenarioInfo.Tags);
            var ignorecase = StringComparer.InvariantCultureIgnoreCase;
            var framework = config.GetVariable("FRAMEWORK", true);
            return !tags.Contains("Selenium", ignorecase) && (framework?.ToUpper() == "PLAYWRIGHT" || tags.Contains("Playwright", ignorecase));
        }

        public static BrowserTypeLaunchPersistentContextOptions GetPlaywrightOptionsWithProfile(IConfiguration config, string profile, string channel = "chrome")
        {
            var options = new BrowserTypeLaunchPersistentContextOptions();
            LoadConfigurationIntoOptions(config, options);
            options.Headless ??= false;
            options.Channel ??= channel;
            options.Args = [$"--profile-directory={profile}"];

            return options;
        }
        public static BrowserTypeLaunchOptions GetPlaywrightOptions(IConfiguration configuration, string channel = "chrome")
        {
            var options = new BrowserTypeLaunchOptions();
            LoadConfigurationIntoOptions(configuration, options);
            options.Headless ??= false;
            options.Channel ??= channel;
            return options;
        }

        public static void LoadConfigurationIntoOptions(IConfiguration config, object options)
        {
            var type = options.GetType();
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (prop.GetSetMethod() == null)
                {
                    continue;
                }
                var value = config.GetVariable($"Playwright.{prop.Name}", true);
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }
                object parsedValue;
                var propTypeName = Nullable.GetUnderlyingType(prop.PropertyType)?.Name ?? prop.PropertyType.Name;
                if (propTypeName == typeof(bool).Name)
                {
                    parsedValue = bool.Parse(value);
                }
                else if (propTypeName == typeof(string).Name)
                {
                    parsedValue = value;
                }
                else if (propTypeName == typeof(Single).Name)
                {
                    parsedValue = Single.Parse(value);
                }
                else if (propTypeName == typeof(ViewportSize).Name)
                {
                    var wh = value.Split(",");
                    var width = 0;
                    var height = 0;
                    try
                    {
                        width = int.Parse(wh[0]);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error Parsing Width of ViewportSize from {value} \n (e.g; Options.ViewportSize: '1200,800')", ex);
                    }
                    try
                    {
                        height = int.Parse(wh[1]);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error Parsing Height of ViewportSize from {value} \n (e.g; Options.ViewportSize: '1200,800')", ex);
                    }
                    parsedValue = new ViewportSize { Width = width, Height = height };

                }
                else
                {
                    throw new NotImplementedException(propTypeName);
                }

                try
                {
                    prop.GetSetMethod()?.Invoke(options, new[] { parsedValue });
                }
                catch (Exception ex)
                {
                    throw new Exception($"error setting playwright option `Playwright.{prop.Name}` value: `{value ?? "null"}`\n", ex);
                }

            }
        }



    }
}
