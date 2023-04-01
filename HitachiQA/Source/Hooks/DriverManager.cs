using BoDi;
using DocumentFormat.OpenXml.Bibliography;
using HitachiQA.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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

namespace HitachiQA.Hooks
{
    [Binding]
    public class DriverManager : HookBase, IDisposable
    {
        public static List<String>? optionsList;

        public BrowserIndicator BrowserIndicator = new BrowserIndicator();
        public IWebDriver WebDriver;


        public DriverManager(IObjectContainer oc, FeatureContext fc, IConfiguration config) : base(oc, fc, config)
        {
            
        }






        [BeforeScenario(Order = 2)]
        public void invokeDriver(FeatureContext FT, ScenarioContext SC, IObjectContainer oc)
        {
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
                var driver = invokeNewDriver(oc, browser);

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

        [BeforeScenario("newWindow", Order = 1)]
        public static void pre_NewWindow()
        {
            throw new NotImplementedException();
        }

        [AfterScenario("newWindow", Order = 1)]
        public static void post_NewWindow()
        {
            throw new NotImplementedException();
        }

        public static ChromeOptions? ChromeOptions;
        public static FirefoxOptions? FirefoxOptions;
        public static EdgeOptions? EdgeOptions;

        public IWebDriver invokeNewDriver(IObjectContainer oc, string browser)
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

            try
            {
                driver.Navigate().GoToUrl(Main.Configuration.GetVariable("HOST"));
            }
            catch(Exception ex) 
            {
                this.Dispose();
                throw new Exception($"Failed navigating to Host {Main.Configuration.GetVariable("HOST", true)}", ex);
            }

            this.WebDriver = driver;
            return driver;

        }

        public void Dispose()
        {
            if(!BrowserIndicator.isNoBrowserFeature)
            {
                try {this.WebDriver.Dispose(); }catch(Exception) { }
            }
        }
    }

    public class BrowserIndicator
    {
       public bool isNoBrowserFeature = false;
    }
}
