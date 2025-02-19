using HitachiQA.Helpers;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using Reqnroll.BoDi;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;
using NetDriverManager = WebDriverManager.DriverManager;

namespace HitachiQA.Hooks.Browsers

{
    [Binding]
    public class WebDriverHook : HookBase, IDisposable
    {
        public static List<string> optionsList;

        public BrowserIndicator BrowserIndicator = new BrowserIndicator();
        public IWebDriver WebDriver { get; set; }
        private TestContext TestContext { get; }
        public WebDriverHook(IObjectContainer oc,
            FeatureContext fc,
            IConfiguration config,
            TestContext tc,
            BrowserIndicator bi
            ) : base(config)
        {
            TestContext = tc;
            BrowserIndicator = bi;
        }

        [BeforeScenario(Order = 2)]
        public void InvokeDriver(FeatureContext FT, ScenarioContext SC, IObjectContainer oc)
        {

            if (!FT.FeatureInfo.Tags.Contains("NoBrowser") && !SC.ScenarioInfo.Tags.Contains("NoBrowser"))
            {
                BrowserIndicator.IsBrowserFeature = true;

                IConfiguration config = oc.Resolve<IConfiguration>();
                var browser = config.GetVariable("BROWSER");
                //if no selenium tag
                //and either driver is playwright or feature tag contains playwright
                //           
                if (ShouldUseSelenium(oc))
                {

                    var driver = InvokeNewSeleniumDriver(browser);
                    WebDriver.NullGuard();
                    oc.RegisterInstanceAs(driver);
                    oc.RegisterInstanceAs(new ScreenShot(FT, SC, driver, TestContext));
                }


            }

        }
        private static bool ShouldUseSelenium(IObjectContainer oc)
        {
            IConfiguration config = oc.Resolve<IConfiguration>();
            FeatureContext FT = oc.Resolve<FeatureContext>();
            var featureTags = FT.FeatureInfo.Tags;
            var ignorecase = StringComparer.InvariantCultureIgnoreCase;
            var framework = config.GetVariable("FRAMEWORK", true);
            return !featureTags.Contains("Playwright", ignorecase) && (framework?.ToUpper() != "PLAYWRIGHT" || featureTags.Contains("Selenium", ignorecase));
        }

        [AfterScenario(Order = 9999)]
        public static void driverCleanup(ObjectContainer oc)
        {
            oc.Resolve<WebDriverHook>().Dispose();
        }

        public static ChromeOptions ChromeOptions;
        public static FirefoxOptions FirefoxOptions;
        public static EdgeOptions EdgeOptions;

        public IWebDriver InvokeNewSeleniumDriver(string browser)
        {
            IWebDriver driver;
            List<string> optionsList = new List<string>();
            string options = Main.Configuration.GetVariable("OPTIONS", true);

            if (options != null)
            {
                //separator is ;
                string[] listArray = options.Split('\x3B');

                foreach (string str in listArray)
                {
                    str.Trim();
                    if (!string.IsNullOrEmpty(str))
                        optionsList.Add(str);
                }
            }

            switch (browser?.ToLower())
            {
                case "chrome":
                    _ = new NetDriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);

                    if (ChromeOptions == null)
                    {
                        ChromeOptions = new ChromeOptions();
                        ChromeOptions.AddArgument("--start-maximized");
                        ChromeOptions.AddArgument("--no-sandbox"); // Bypass OS security model
                        ChromeOptions.AddArgument("--disable-dev-shm-usage");
                        ChromeOptions.AddArgument("--disable-extensions");
                        ChromeOptions.AddArgument("--force-device-scale-factor=1.5");
                        ChromeOptions.AddUserProfilePreference("profile.cookie_controls_mode", "0");
                        ChromeOptions.AddArguments(optionsList);
                    }

                    Log.Info("initializing chromedriver");
                    driver = new ChromeDriver(ChromeOptions);
                    Log.Info("initialized chromedriver");
                    break;

                case "firefox":
                    _ = new NetDriverManager().SetUpDriver(new FirefoxConfig(), VersionResolveStrategy.Latest);
                    if (FirefoxOptions == null)
                    {
                        FirefoxOptions = new FirefoxOptions();
                        FirefoxOptions.AddArgument("--no-sandbox");
                        FirefoxOptions.AddArgument("--start-maximized");
                        FirefoxOptions.AcceptInsecureCertificates = true;
                        FirefoxOptions.AddArguments(optionsList);
                    }

                    driver = new FirefoxDriver(FirefoxOptions);
                    break;

                case "edge":
                    _ = new NetDriverManager().SetUpDriver(new EdgeConfig(), VersionResolveStrategy.Latest);
                    if (EdgeOptions == null)
                    {
                        EdgeOptions = new EdgeOptions();
                        EdgeOptions.AddArgument("--no-sandbox");
                        EdgeOptions.AddArgument("--start-maximized");
                        EdgeOptions.AddArguments(optionsList);
                    }

                    driver = new EdgeDriver(EdgeOptions);
                    break;

                default:
                    if (string.IsNullOrWhiteSpace(browser))
                    {
                        throw new InvalidOperationException("BROWSER variable was not set, most likely forgot to select a .runsettings file. Refer to README for more info");
                    }
                    throw new NotImplementedException($"Environment variable BROWSER value={browser} is not supported");
            }
            WebDriver = driver;

            try
            {
                driver.Navigate().GoToUrl(Main.Configuration.GetVariable("HOST"));
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed navigating to Host {Main.Configuration.GetVariable("HOST", true)}", ex);
            }

            return driver;

        }

        public void Dispose()
        {
            if (BrowserIndicator.IsBrowserFeature)
            {
                var currentLogSev = Severity.parseLevel(Configuration.GetSection("Logging").GetSection("LogLevel")["Default"] ?? "Debug");
                if (currentLogSev.Level == Severity.DEBUG.Level)
                {
                    return;
                }
                try { WebDriver?.Dispose(); } catch (Exception) { }
            }
        }
    }


}
