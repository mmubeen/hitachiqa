using BoDi;
using HitachiQA.Driver;
using HitachiQA.Hooks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using HitachiQA.Helpers;
using FluentAssertions.Extensions;
using HitachiQA.Source.Helpers;

namespace HitachiQA.UnitTests.StepDefinitions
{
    [Binding]
    public class WebDriversStepDefinitions
    {
        ObjectContainer ObjectContainer;

        UserActions UserActions => ObjectContainer.Resolve<UserActions>();
        DriverManager DriverManager;

        public WebDriversStepDefinitions(ObjectContainer objectContainer, DriverManager DriverManager)
        {
            ObjectContainer = objectContainer;
            this.DriverManager = DriverManager;
        }

        [Given(@"Browser is up")]
        public void GivenBrowserIsUp()
        {
            this.ObjectContainer.Resolve<OpenQA.Selenium.IWebDriver>().Should().NotBeNull();
        }

        [Then(@"user should land on HSAL homepage")]
        public void ThenUserShouldLandOnHSALHomepage()
        {
            new Element(By.XPath("//*[contains(text(), 'Hitachi')]"), UserActions).assertElementIsPresent();
            UserActions.OpenNewTab();
            new Element(By.XPath("//*[contains(text(), 'Hitachi')]"), UserActions).assertElementIsPresent();
            new Element(By.XPath("//*[@title='Open mobile navigation']"), UserActions).TryClick();

            new Element(By.XPath("//*[contains(text(), 'Contact us')]"), UserActions).Click();
            var contactUsElement = new Element(By.XPath("//*[@data-formtitle='Contact Us']"), UserActions);
            contactUsElement.assertElementIsPresent(5);

            UserActions.SwitchContext();
            Log.Info(UserActions.Title);
            //DriverManager.SwitchWindowContext();
            contactUsElement.assertElementNotPresent(5);

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
           
        }

        [Given(@"user loads option ""([^""]*)"" into the browser")]
        public void GivenUserLoadsOptionIntoTheBrowser(string option)
        {
            DriverManager.ChromeOptions = new ChromeOptions();
            DriverManager.ChromeOptions.AddArgument("--headless");
            DriverManager.ChromeOptions.AddArgument(option);
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
        }
    }
}
