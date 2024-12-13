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
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HitachiQA.Hooks.Browsers
{
    [Binding]
    public class PlaywrightHook : HookBase
    {
        public static List<String>? optionsList;

        public BrowserIndicator BrowserIndicator { get; init; }
        public IPlaywright? Playwright { get; set; }
        public IPage? PlaywrightPage { get; set; }
        public IBrowser? PlaywrightBrowser { get; set; }
        public IBrowserContext? PlaywrightBrowserContext { get; set; }
        private TestContext TestContext { get;  }
        public PlaywrightHook(
            IObjectContainer oc, 
            FeatureContext fc, 
            IConfiguration config, 
            TestContext tc,
            BrowserIndicator bi
            ) : base(oc, fc, config)
        {
            TestContext = tc;
            BrowserIndicator= bi;
        }

        public async Task InvokeBrowserAsync(string browserName, string host, string profile=null)
        {
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            if(profile!=null)
            {
                PlaywrightBrowser = await InvokeNewPlaywrightBrowserAsync(browserName, profile);
            }
            else
            {
                PlaywrightBrowser = await InvokeNewPlaywrightBrowserAsync(browserName);
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
            if (installPlaywright?.ToUpper()=="TRUE" || framework?.ToUpper()=="PLAYWRIGHT")
            {
                Microsoft.Playwright.Program.Main(["install", "--with-deps"]);
            }
        }

        [BeforeScenario(Order = 2)]
        public async Task InvokeBrowserAsync(FeatureContext fc, ScenarioContext sc, IObjectContainer oc)
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
                    oc.RegisterInstanceAs<ScreenShot>(new ScreenShot(PlaywrightPage, TestContext));
                    await PlaywrightPage.GotoAsync("/");
                }
            }

        }

        [AfterScenario]
        public async Task closeContext()
        {
            if (this.PlaywrightPage != null && PlaywrightPage.Video != null)
            {
                var videoPath = await PlaywrightPage.Video.PathAsync();
                this.TestContext.AddResultFile(videoPath);
                await PlaywrightBrowserContext?.CloseAsync();
                Console.WriteLine($"\nVideo: {new Uri(videoPath)}\n");

            }
        }

        public async Task<IBrowser> InvokeNewPlaywrightBrowserAsync(string browserName, string? profile=null)
        {
            if (Playwright == null)
                throw new Exception("Playwright must be initialized before invoking browser");

            IBrowser browser;
            switch (browserName.ToLower())
            {
                case "chrome":
                    browser =  await Playwright.Chromium.LaunchAsync(GetPlaywrightOptions("chrome"));
                    break;
                case "msedge":
                    if (profile != null)
                    {
                        if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            throw new NotImplementedException("InvokeNewPlaywrightBrowserAsync with profile only works on windows");
                        }
                        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        var userData = Path.Combine(userProfile, "AppData\\Local\\Microsoft\\Edge\\User%20Data\\");
                        var context = await Playwright
                            .Chromium
                            .LaunchPersistentContextAsync(userData, GetPlaywrightOptionsWithProfile(profile, "msedge"));
                        this.PlaywrightBrowserContext = context;
                        return context.Browser;
                    }
                    else
                    {
                        browser = await Playwright.Chromium.LaunchAsync(GetPlaywrightOptions("msedge"));
                    }
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

        private static bool ShouldUsePlaywright(FeatureContext fc, ScenarioContext sc, IConfiguration config)
        {
            var tags = fc.FeatureInfo.Tags.Concat(sc.ScenarioInfo.Tags);
            var ignorecase = StringComparer.InvariantCultureIgnoreCase;
            var framework = config.GetVariable("FRAMEWORK", true);
            return !tags.Contains("Selenium", ignorecase) && (framework?.ToUpper() == "PLAYWRIGHT" || tags.Contains("Playwright", ignorecase));
        }

        private BrowserTypeLaunchPersistentContextOptions GetPlaywrightOptionsWithProfile(string profile, string channel = "chrome")
        {
            var options = new BrowserTypeLaunchPersistentContextOptions();
            var props = options.GetType().Properties();
            foreach (var prop in props)
            {
                if (prop.GetSetMethod() == null)
                {
                    continue;
                }
                var value = Configuration.GetVariable($"Playwright.{prop.Name}", true);
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

            options.Headless ??= false;
            options.Channel ??= channel;
            options.Args =[$"--profile-directory={profile}"];

            return options;
        }

        private BrowserTypeLaunchOptions GetPlaywrightOptions(string channel="chrome")
        {
            var options = new BrowserTypeLaunchOptions();
            var props = options.GetType().Properties();
            foreach (var prop in props)
            {
                if(prop.GetSetMethod()==null) {
                    continue;
                }
                var value = Configuration.GetVariable($"Playwright.{prop.Name}", true);
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
            options.Channel ??= channel;
            
            return options;
        }

    }
}
