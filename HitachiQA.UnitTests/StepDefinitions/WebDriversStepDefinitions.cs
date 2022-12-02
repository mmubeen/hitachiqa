using BoDi;
using HitachiQA.Driver;
using System;
using TechTalk.SpecFlow;
using HitachiQA.Hooks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using DocumentFormat.OpenXml.Bibliography;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using WebDriverManager.DriverConfigs.Impl;

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
            this.ObjectContainer.Resolve<IWebDriver>().Should().NotBeNull();
        }

        [Then(@"user should land on HSAL homepage")]
        public void ThenUserShouldLandOnHSALHomepage()
        {
            new Element(By.XPath("//*[contains(text(), 'Hitachi Solutions')]"), UserActions).assertElementIsPresent();
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
            var driver = ObjectContainer.Resolve<IWebDriver>();
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

      


    }
}
