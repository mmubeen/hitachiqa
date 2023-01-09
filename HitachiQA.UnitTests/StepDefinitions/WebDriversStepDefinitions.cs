using BoDi;
using HitachiQA.Driver;
using HitachiQA.Hooks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using HitachiQA.Helpers;
using FluentAssertions.Extensions;

namespace HitachiQA.UnitTests.StepDefinitions
{
    [Binding]
    public class WebDriversStepDefinitions
    {
        ObjectContainer ObjectContainer;

        UserActions UserActions => ObjectContainer.Resolve<UserActions>();

        public WebDriversStepDefinitions(ObjectContainer objectContainer)
        {
            ObjectContainer = objectContainer;
        }

        [Given(@"Browser is up")]
        public void GivenBrowserIsUp()
        {
            this.ObjectContainer.Resolve<OpenQA.Selenium.IWebDriver>().Should().NotBeNull();
        }

        [Then(@"user should land on HSAL homepage")]
        public void ThenUserShouldLandOnHSALHomepage()
        {
            new Element(By.XPath("//*[contains(text(), 'Hitachi Solutions')]"), UserActions).assertElementIsPresent();
            this.ObjectContainer.Resolve<ScreenShot>().Take(Severity.INFO);
        }


        [Then(@"UserActions instance should be added to the Object Container")]
        public void ThenWebDriverInstanceShouldBeAddedToTheObjectContainer()
        {
            UserActions.Should().NotBeNull();
        }

        [When(@"""([^""]*)"" is invoked")]
        public void WhenIsInvoked(string browser)
        {
            DriverManager.invokeNewDriver(ObjectContainer, browser);
        }



        [Then(@"verify ""([^""]*)"" is open")]
        public void ThenVerifyIsOpen(string browser)
        {
            var driver = ObjectContainer.Resolve<OpenQA.Selenium.IWebDriver>();
            switch (browser?.ToLower())
            {
                case "chrome":
                    driver.Should().BeOfType<ChromeDriver>();
                    break;

                case "firefox":
                    driver.Should().BeOfType<FirefoxDriver>();

                    break;

                case "edge":
                    driver.Should().BeOfType<EdgeDriver>();

                    break;

                default:
                    throw new NotImplementedException($"BROWSER value={browser} is not supported");
            }
            driver.Dispose();

           
        }

        [Given(@"user loads option ""([^""]*)"" into the browser")]
        public void GivenUserLoadsOptionIntoTheBrowser(string option)
        {
            Main.Configuration["OPTIONS"] = (Main.Configuration["OPTIONS"] + $"; {option}").Trim(';');
        }

        [Then(@"""([^""]*)"" should be set to the browser")]
        public void ThenShouldBeSet(string option)
        {
            var js = ObjectContainer.Resolve<JSExecutor>();

            switch (option)
            {
                case "--start-maximized":
                    var fullScreenEnabled = (bool)js.execute("return document.fullscreenEnabled");
                    fullScreenEnabled.Should().BeTrue();    
                    break;
     
                case "--window-size=840,640":
                    var height = (long)js.execute("return window.outerHeight");
                    var width = (long)js.execute("return window.outerWidth");
                    height.Should().Be(640);
                    width.Should().Be(840);
                    break;
                default: throw new NotImplementedException(option);
            }
            ObjectContainer.Resolve<OpenQA.Selenium.IWebDriver>().Dispose();

        }





    }
}
