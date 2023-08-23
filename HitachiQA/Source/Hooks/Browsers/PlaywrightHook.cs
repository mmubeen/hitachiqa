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
using HitachiQA.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace HitachiQA.Hooks.Browsers
{
    [Binding]
    public class PlaywrightHook : HookBase
    {
        public static List<String>? optionsList;

        public BrowserIndicator BrowserIndicator { get; init; }
        private IPage? PlaywrightPage { get; set; }
        private IBrowser? PlaywrightBrowser { get; set; }
        private IBrowserContext? PlaywrightBrowserContext { get; set; }
        private TestContext TestContext { get;  }
        public PlaywrightHook(
            IObjectContainer oc, 
            FeatureContext fc, 
            IConfiguration config, 
            TestContext tc,
            BrowserIndicator bi
            ) : base(oc, fc, config)
        {
            this.TestContext = tc;
            BrowserIndicator= bi;
        }

        [BeforeScenario(Order = 2)]
        public void invokeDriver(FeatureContext FT, ScenarioContext SC, IObjectContainer oc)
        {
            if (!FT.FeatureInfo.Tags.Contains("NoBrowser") && !SC.ScenarioInfo.Tags.Contains("NoBrowser"))
            {

                BrowserIndicator.IsBrowserFeature = true;
                //if no selenium tag
                //and either driver is playwright or feature tag contains playwright
                //           
                if (ShouldUsePlaywright(oc))
                {

                    PlaywrightBrowser = oc.Resolve<IBrowser>();
                    PlaywrightBrowserContext = PlaywrightBrowser.CreateNewContext(Configuration.GetVariable("HOST"));
                    PlaywrightPage = PlaywrightBrowserContext.CreateNewPage();
                    oc.RegisterInstanceAs<IBrowserContext>(PlaywrightBrowserContext);
                    oc.RegisterInstanceAs<IPage>(PlaywrightPage);
                    oc.RegisterInstanceAs<ScreenShot>(new ScreenShot(PlaywrightPage, TestContext));
                    PlaywrightPage.GotoAsync("/");
                }
            }

        }

        private static bool ShouldUsePlaywright(IObjectContainer oc)
        {
            IConfiguration config = oc.Resolve<IConfiguration>();
            FeatureContext FT = oc.Resolve<FeatureContext>();
            var featureTags = FT.FeatureInfo.Tags;
            var ignorecase = StringComparer.InvariantCultureIgnoreCase;
            var framework = config.GetVariable("FRAMEWORK", true);
            return !featureTags.Contains("Selenium", ignorecase) && (framework?.ToUpper() == "PLAYWRIGHT" || featureTags.Contains("Playwright", ignorecase));
        }

        [BeforeFeature(Order = 2)]
        public static async Task invokePlaywrightBrowser(FeatureContext FT, IObjectContainer oc)
        {

            var tags = FT.FeatureInfo.Tags;
            var ignorecase = StringComparer.InvariantCultureIgnoreCase;
            if (!tags.Contains("NoBrowser", ignorecase))
            { 

                IConfiguration config = oc.Resolve<IConfiguration>();
                var browser = config.GetVariable("BROWSER");
                
                if(ShouldUsePlaywright(oc))
                {
                    oc.RegisterInstanceAs<IPlaywright>(await Microsoft.Playwright.Playwright.CreateAsync());

                    var b = InvokeNewPlaywrightBrowser(oc, browser);
                    oc.RegisterInstanceAs<IBrowser>(b);

                }
            }
        }
        [AfterScenario]
        public void closeContext()
        {
            if(this.PlaywrightPage != null && PlaywrightPage.Video!=null)
            {
                var videoPath = PlaywrightPage.Video.PathAsync().Result;
                this.TestContext.AddResultFile(videoPath);
                PlaywrightBrowserContext?.CloseAsync().Wait();
                Console.WriteLine($"\nVideo: {new Uri(videoPath)}\n");

            }
        }

        public static IBrowser InvokeNewPlaywrightBrowser(IObjectContainer oc, string browserName)
        {
            var engine = oc.Resolve<IPlaywright>();
            IBrowser browser;
            switch (browserName.ToLower())
            {
                case "chrome":
                        browser = engine.Chromium.LaunchAsync(GetPlaywrightOptions(oc)).Result;
                    break;
                default:
                    if (string.IsNullOrWhiteSpace(browserName))
                    {
                        throw new InvalidOperationException("BROWSER variable was not set, most likely forgot to select a .runsettings file. Refer to README for more info");
                    }
                    throw new NotImplementedException($"BROWSER value={browserName} is not supported");
            }

            return browser;


        }

        private static BrowserTypeLaunchOptions GetPlaywrightOptions(IObjectContainer oc)
        {
            var config = oc.Resolve<IConfiguration>();
            var options = new BrowserTypeLaunchOptions();
            var props = options.GetType().Properties();
            foreach (var prop in props)
            {
                if(prop.GetSetMethod()==null) {
                    continue;
                }
                var value = config.GetVariable($"Playwright.{prop.Name}", true);
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }
                object parsedValue;
                var propTypeName = Nullable.GetUnderlyingType(prop.PropertyType)?.Name??prop.PropertyType.Name;
                if (propTypeName == typeof(bool).Name) {
                    parsedValue = bool.Parse(value);
                }
                else if(propTypeName == typeof(string).Name) {
                    parsedValue = value;
                }
                else {
                    throw new NotImplementedException(propTypeName);
                }
               
                try
                {
                    prop.GetSetMethod()?.Invoke(options, new[] { parsedValue });
                }
                catch(Exception ex)
                {
                    throw new Exception($"error setting playwright option `Playwright.{prop.Name}` value: `{value ?? "null"}`\n", ex);
                }
                
            }

            options.Headless ??= false;
            options.Channel ??= "chrome";
            
            return options;
            

        }

    }
}
