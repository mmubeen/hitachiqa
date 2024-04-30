using BoDi;
using HitachiQA.Driver;
using HitachiQA.Hooks.Browsers;
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
        WebDriverHook WebDriverHook;

        public WebDriversStepDefinitions(ObjectContainer objectContainer, WebDriverHook WDH)
        {
            ObjectContainer = objectContainer;
            WebDriverHook = WDH;
        }

        [Given(@"Browser is up")]
        public void GivenBrowserIsUp()
        {
            ObjectContainer.Resolve<OpenQA.Selenium.IWebDriver>().Should().NotBeNull();
        }

        [Then(@"user should land on HSAL homepage selenium")]
        public void ThenUserShouldLandOnHSALHomepage()
        {
            new Element(By.XPath("//*[contains(text(), 'Hitachi')]"), UserActions).assertElementIsPresent();
            UserActions.OpenNewTab();
            new Element(By.XPath("//*[contains(text(), 'Hitachi')]"), UserActions).assertElementIsPresent();
            
            if(new Element(By.XPath("//*[@title='Open mobile navigation']"), UserActions).TryClick())
            {
                new Element(By.XPath("(//*[contains(text(), 'Contact us')])[2]"), UserActions).Click();
            }
            else
            {
                new Element(By.XPath("(//*[contains(text(), 'Contact us')])[1]"), UserActions).Click();
            }

            var firstNameFormInput = new Element(By.XPath("//*[text()='First Name']/..//input"), UserActions);
            firstNameFormInput.assertElementIsPresent(5);

            UserActions.SwitchContext();
            Log.Info(UserActions.Title);
            //DriverManager.SwitchWindowContext();
            firstNameFormInput.assertElementNotPresent(5);

            ObjectContainer.Resolve<ScreenShot>().Take(Severity.INFO);
        }


        [Then(@"UserActions instance should be added to the Object Container")]
        public void ThenWebDriverInstanceShouldBeAddedToTheObjectContainer()
        {
            UserActions.Should().NotBeNull();
        }

        [When(@"""([^""]*)"" is invoked")]
        public void WhenIsInvoked(string browser)
        {
            WebDriverHook.invokeNewSeleniumDriver(ObjectContainer, browser);
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
            WebDriverHook.ChromeOptions = new ChromeOptions();
            WebDriverHook.ChromeOptions.AddArgument("--headless");
            WebDriverHook.ChromeOptions.AddArgument(option);
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
