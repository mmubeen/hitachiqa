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
    public class DriverManager : HookBase
    {
        public static List<String>? optionsList;

        public BrowserIndicator BrowserIndicator;

        public DriverManager(IObjectContainer oc, FeatureContext fc, IConfiguration config) : base(oc, fc, config)
        {
            
        }


        [BeforeScenario(Order = 2)]
        public void invokeDriver(FeatureContext FT, ScenarioContext SC, ObjectContainer oc)
        {

            BrowserIndicator = new BrowserIndicator();
            if (!FT.FeatureInfo.Tags.Contains("NoBrowser") && !SC.ScenarioInfo.Tags.Contains("NoBrowser"))
            {
                BrowserIndicator.isNoBrowserFeature = false;
                
                var config = oc.Resolve<IConfiguration>();
                var browser = config.GetVariable("BROWSER");                                 

                invokeNewDriver(oc, browser);
                


            }
            else
            {
                BrowserIndicator.isNoBrowserFeature = true;
            }
            oc.RegisterInstanceAs<BrowserIndicator>(BrowserIndicator);

        }

        [AfterScenario]
        public void handleScreenshot(ScenarioContext SC)
        {
            if (!BrowserIndicator.isNoBrowserFeature && SC.TestError != null)
            {
               this.ObjectContainer.Resolve<ScreenShot>().Error();
            }
        }


        [AfterScenario(Order =9999)]
        public static void driverCleanup(ObjectContainer oc)
        {
            if (!oc.Resolve<BrowserIndicator>().isNoBrowserFeature && Severity.parseLevel(oc.Resolve<IConfiguration>().GetSection("Logging").GetSection("LogLevel")["Default"]) != Severity.DEBUG)
            {
                oc.Resolve<IWebDriver>().Dispose();
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

        public static void invokeNewDriver(ObjectContainer oc, string browser)
        {
            IWebDriver driver;
            List<string> optionsList= new List<string>();
            String? options = Main.Configuration.GetVariable("OPTIONS", true);

            if (options != null)
            {
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
                    var cOptions = new ChromeOptions();


                    cOptions.AddArgument("--start-maximized");
                    cOptions.AddArgument("--no-sandbox"); // Bypass OS security model
                    cOptions.AddUserProfilePreference("profile.cookie_controls_mode", "0");
                    cOptions.AddArguments(optionsList);                    

                    driver = new ChromeDriver(cOptions);
                    break;

                case "firefox":
                    _ = new NetDriverManager().SetUpDriver(new FirefoxConfig(), VersionResolveStrategy.Latest);
                    var fOptions = new FirefoxOptions();
                    fOptions.AddArgument("--no-sandbox");
                    driver = new FirefoxDriver(fOptions);
                    break;

                case "edge":
                    _ = new NetDriverManager().SetUpDriver(new EdgeConfig(), VersionResolveStrategy.MatchingBrowser);
                    EdgeOptions edgeOptions = new EdgeOptions();
                    edgeOptions.AddArgument("--no-sandbox");
                    driver = new EdgeDriver(edgeOptions);
                    break;

                default:
                    if(string.IsNullOrWhiteSpace(browser))
                    {
                        throw new InvalidOperationException("BROWSER variable was not set, most likely forgot to select a .runsettings file. Refer to README for more info");
                    }
                    throw new NotImplementedException($"Environment variable BROWSER value={browser} is not supported");
            }
            driver.Navigate().GoToUrl(Main.Configuration.GetVariable("HOST"));
            oc.RegisterInstanceAs<IWebDriver>(driver);
            
        }

      

    }

    public class BrowserIndicator
    {
       public bool isNoBrowserFeature = false;
    }
}
