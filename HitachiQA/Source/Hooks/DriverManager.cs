using BoDi;
using DocumentFormat.OpenXml.Bibliography;
using HitachiQA.Helpers;
using HitachiQA.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;
using NetDriverManager = WebDriverManager.DriverManager;
using HitachiQA.Source.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HitachiQA.Hooks
{
    [Binding]
    public class DriverManager : HookBase, IDisposable
    {
        public static List<String>? optionsList;

        public BrowserIndicator BrowserIndicator = new BrowserIndicator();
        private IWebDriver? WebDriver { get; set; }
        private IPage? PlaywrightPage { get; set; }
        private IBrowser? PlaywrightBrowser { get; set; }
        private IBrowserContext? PlaywrightBrowserContext { get; set; }
        private IPlaywright? PlaywrightEngine { get; set; }
        private TestContext TestContext { get;  }
        public DriverManager(IObjectContainer oc, FeatureContext fc, IConfiguration config, TestContext tc) : base(oc, fc, config)
        {
            this.TestContext = tc;
        }

        [BeforeScenario(Order = 2)]
        public void invokeDriver(FeatureContext FT, ScenarioContext SC, IObjectContainer oc)
        {
            var featureTags = FT.FeatureInfo.Tags;
            var ignorecase = StringComparer.InvariantCultureIgnoreCase;
            oc.RegisterInstanceAs<BrowserIndicator>(BrowserIndicator);

            if (FT.FeatureInfo.Tags.Contains("NoBrowser") || SC.ScenarioInfo.Tags.Contains("NoBrowser"))
            {
                BrowserIndicator.isNoBrowserFeature = true;
            }
            else
            {
                BrowserIndicator.isNoBrowserFeature = false;

                IConfiguration config= oc.Resolve<IConfiguration>();
                var browser = config.GetVariable("BROWSER");
                var driver = config.GetVariable("DRIVER", true);

                //if no selenium tag
                //and either driver is playwright or feature tag contains playwright
                //           
                if (!featureTags.Contains("Selenium", ignorecase) && (driver?.ToUpper() == "PLAYWRIGHT" || featureTags.Contains("Playwright", ignorecase)))
                {
                    PlaywrightEngine = oc.Resolve<IPlaywright>();

                    PlaywrightBrowser = oc.Resolve<IBrowser>();
                    PlaywrightBrowserContext = PlaywrightBrowser.CreateNewContext();
                    PlaywrightPage = PlaywrightBrowserContext.CreateNewPage();
                    oc.RegisterInstanceAs<IBrowserContext>(PlaywrightBrowserContext);
                    oc.RegisterInstanceAs<IPage>(PlaywrightPage);
                    PlaywrightPage.GotoAsync(Main.Configuration.GetVariable("HOST")).Wait();
                    oc.RegisterInstanceAs<ScreenShot>(new ScreenShot(PlaywrightPage, TestContext));
                }
                else
                {
                    invokeNewSeleniumDriver(oc, browser);
                    WebDriver.NullGuard();
                    oc.RegisterInstanceAs<ScreenShot>(new ScreenShot(WebDriver, TestContext));

                }


            }

        }
        [BeforeFeature(Order = 2)]
        public static void invokePlaywrightBrowser(FeatureContext FT, IObjectContainer oc)
        {
            var tags = FT.FeatureInfo.Tags;
            var ignorecase = StringComparer.InvariantCultureIgnoreCase;
            if (!tags.Contains("NoBrowser", ignorecase))
            { 

                IConfiguration config = oc.Resolve<IConfiguration>();
                var browser = config.GetVariable("BROWSER");
                var driver = config.GetVariable("DRIVER", true);
                
                if(!tags.Contains("Selenium", ignorecase) && (driver?.ToUpper() == "PLAYWRIGHT" || tags.Contains("Playwright", ignorecase)))
                {
                    var b = InvokeNewPlaywrightBrowser(oc, browser);
                    oc.RegisterInstanceAs<IBrowser>(b);

                }
            }
        }
        [AfterScenario]
        public void closeContext(IObjectContainer oc)
        {
            if(this.PlaywrightPage != null && PlaywrightPage.Video!=null)
            {
                var videoPath = PlaywrightPage.Video.PathAsync().Result;
                this.TestContext.AddResultFile(videoPath);
                PlaywrightBrowserContext?.CloseAsync().Wait();
                Console.WriteLine($"\nVideo: {new Uri(videoPath)}\n");

            }
        }

        [AfterScenario(Order =1)]
        public void handleScreenshot(ScenarioContext SC)
        {
            if (!BrowserIndicator.isNoBrowserFeature && SC.TestError != null)
            {
                try{
                    this.ObjectContainer.Resolve<ScreenShot>().Error();

                }
                catch(Exception ex)
                {
                    Log.Warn($"error taking screenshot\n {ex.Message} \n{ex.StackTrace}");
                }
            }
        }


        [AfterScenario(Order =9999)]
        public static void driverCleanup(ObjectContainer oc)
        {

            //if no browser feature, return
            if (oc.Resolve<BrowserIndicator>().isNoBrowserFeature)
            {
                return;
            }
            Severity currentLogSev;
            try
            {
                currentLogSev = Severity.parseLevel(oc.Resolve<IConfiguration>().GetSection("Logging").GetSection("LogLevel")["Default"]);
                if (currentLogSev == Severity.DEBUG)
                {
                    return;
                }
            }
            finally
            {
                //oc.Resolve<DriverManager>().Dispose();
            }

        }

        public static ChromeOptions? ChromeOptions;
        public static FirefoxOptions? FirefoxOptions;
        public static EdgeOptions? EdgeOptions;

        public IWebDriver invokeNewSeleniumDriver(IObjectContainer oc, string browser)
        {
            IWebDriver driver;
            List<string> optionsList= new List<string>();
            String? options = Main.Configuration.GetVariable("OPTIONS", true);

            if (options != null)
            {
                //separator is ;
                String[] listArray = options.Split('\x3B');

                foreach (String str in listArray)
                {
                    str.Trim();
                    if(!string.IsNullOrEmpty(str))
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
                        ChromeOptions.AddUserProfilePreference("profile.cookie_controls_mode", "0");
                        ChromeOptions.AddArguments(optionsList);
                    }

                    Log.Info("initializing chromedriver");
                    driver = new ChromeDriver(ChromeOptions);
                    Log.Info("initialized chromedriver");
                    break;

                case "firefox":
                    _ = new NetDriverManager().SetUpDriver(new FirefoxConfig(), VersionResolveStrategy.Latest);
                    if(FirefoxOptions == null)
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
                    _ = new NetDriverManager().SetUpDriver(new EdgeConfig(), VersionResolveStrategy.MatchingBrowser);
                    if(EdgeOptions==null)
                    {
                        EdgeOptions = new EdgeOptions();
                        EdgeOptions.AddArgument("--no-sandbox");
                        EdgeOptions.AddArgument("--start-maximized");
                        EdgeOptions.AddArguments(optionsList);
                    }

                    driver = new EdgeDriver(EdgeOptions);
                    break;

                default:
                    if(string.IsNullOrWhiteSpace(browser))
                    {
                        throw new InvalidOperationException("BROWSER variable was not set, most likely forgot to select a .runsettings file. Refer to README for more info");
                    }
                    throw new NotImplementedException($"Environment variable BROWSER value={browser} is not supported");
            }
            oc.RegisterInstanceAs<IWebDriver>(driver, null, true);
            this.WebDriver = driver;

            try
            {
                driver.Navigate().GoToUrl(Main.Configuration.GetVariable("HOST"));
            }
            catch(Exception ex) 
            {
                throw new Exception($"Failed navigating to Host {Main.Configuration.GetVariable("HOST", true)}", ex);
            }

            return driver;

        }
        public static IBrowser InvokeNewPlaywrightBrowser(IObjectContainer oc, string browserName)
        {
            var engine = oc.Resolve<IPlaywright>();
            IBrowser browser;
            switch (browserName.ToLower())
            {
                case "chrome":
                    #if DEBUG
                        browser = engine.Chromium.LaunchAsync(new() { Headless=false, Channel="chrome"}).Result;
                    #else
                        browser = engine.Chromium.LaunchAsync(new() { Headless=true, Channel="chrome"}).Result;
                    #endif
                    break;
                default:
                    if (string.IsNullOrWhiteSpace(browserName))
                    {
                        throw new InvalidOperationException("BROWSER variable was not set, most likely forgot to select a .runsettings file. Refer to README for more info");
                    }
                    throw new NotImplementedException($"Environment variable BROWSER value={browserName} is not supported");
            }

            return browser;


        }

        public void Dispose()
        {
            if(!BrowserIndicator.isNoBrowserFeature)
            {
                try {this.WebDriver?.Dispose(); }catch(Exception) { }
                //try { this.PlaywrightEngine?.Dispose(); } catch (Exception) { }

            }
        }
        [BeforeTestRun]
        public static async Task InvokePlaywright(IObjectContainer container)
        {
            container.RegisterInstanceAs<IPlaywright>(await Playwright.CreateAsync());
        }
    }

    public class BrowserIndicator
    {
       public bool isNoBrowserFeature = false;
    }
}
