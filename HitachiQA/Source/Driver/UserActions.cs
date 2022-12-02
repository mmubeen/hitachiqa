using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using HitachiQA.Helpers;
using System.Linq;
using OpenQA.Selenium.Interactions;
using Microsoft.Extensions.Configuration;
using BoDi;

namespace HitachiQA.Driver
{
    public class UserActions
    {
        private readonly IWebDriver WebDriver;
        private readonly IConfiguration Configuration;
        private readonly JSExecutor JSExecutor;
        public UserActions(ObjectContainer objectContainer)
        {
            this.WebDriver = objectContainer.Resolve<IWebDriver>();
            this.Configuration = objectContainer.Resolve<IConfiguration>();
            this.JSExecutor = objectContainer.Resolve<JSExecutor>();

            var configKeys = this.Configuration.GetChildren();
            var wait = configKeys.FirstOrDefault(it => it.Key == "DEFAULT_WAIT_SECONDS");
            var highlight = configKeys.FirstOrDefault(it => it.Key == "HIGHLIGHT_ON");
            var loadingXPath = configKeys.FirstOrDefault(it => it.Key == "LOADING_SCREEN_XPATH");

            if(wait!= null){
                DEFAULT_WAIT_SECONDS = int.Parse(wait.Value);
            }
            if (highlight != null){
                HIGHLIGHT_ON = bool.Parse(highlight.Value);
            }
            if (loadingXPath != null){
                LOADING_SCREEN_XPATH = loadingXPath.Value;
            }
        }

        public int DEFAULT_WAIT_SECONDS = 30;

        public bool HIGHLIGHT_ON = false;

        //Most applications have some sort of loading screen, please allow this variable to hold the that locator. please set this xpath in your .env.json file
        private readonly String LOADING_SCREEN_XPATH = "";

        public int ProcessWaitParam(int? wait) => (int)(wait == null ? DEFAULT_WAIT_SECONDS : wait);


        public void waitForPageLoad()
        {
            if(!string.IsNullOrWhiteSpace(LOADING_SCREEN_XPATH))
            {
                By locator = By.XPath(LOADING_SCREEN_XPATH);
                //this is optional
                try
                {
                    FindElementWaitUntilVisible(locator, 1);
                    WaitForElementToDisappear(locator, 120);
                }
                catch(Exception)
                {
                    //do nothing
                }
            }
        }
  

        public void Navigate(string URL_OR_PATH, params (string key, string value)[] parameters)
        {
            var URL = Functions.ParseURL(URL_OR_PATH, parameters);
            Log.Info("Navigate to: " + URL);

            Navigate(URL);
        }
        
        public void Navigate(string URL_OR_PATH)
        {
            var URL = Functions.ParseURL(URL_OR_PATH);
            Log.Info("Navigate to: " + URL);
           
            this.WebDriver.Navigate().GoToUrl(URL);           
        }

        public string GetCurrentURL()
        {
            return this.WebDriver.Url;
        }

        public void Refresh()
        {
            this.WebDriver.Navigate().Refresh();
            WaitForSpinnerToDisappear();
        }

        public void Back()
        {
            this.WebDriver.Navigate().Back();
            WaitForSpinnerToDisappear();
        }

        //
        // General Element Actions
        //

        public string getElementText(By ElementLocator, int? wait_Seconds = null)
        {
            var textField = FindElementWaitUntilVisible(ElementLocator, ProcessWaitParam(wait_Seconds));
            return textField.Text.Trim();
        }

        public bool Click(By ElementLocator, int? wait_Seconds = null, bool optional = false)
        {
            try
            {
                try
                {
                    FindElementWaitUntilClickable(ElementLocator, ProcessWaitParam(wait_Seconds)).Click();
                }
                catch (StaleElementReferenceException)
                {
                    Thread.Sleep(1000);
                    FindElementWaitUntilClickable(ElementLocator, ProcessWaitParam(wait_Seconds)).Click();

                }
                catch (ElementClickInterceptedException)
                {
                    waitForPageLoad();

                    FindElementWaitUntilClickable(ElementLocator, ProcessWaitParam(wait_Seconds)).Click();
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                Functions.handleFailure($"Locator: {ElementLocator}", ex, optional);
                return false;
            }
            return true;
        }

        public bool DoubleClick(By ElementLocator, int? wait_Seconds = null, bool optional = false)
        {
            Actions Action = new Actions(this.WebDriver);
            try
            {
                try
                {
                    var element = FindElementWaitUntilClickable(ElementLocator, ProcessWaitParam(wait_Seconds));
                    Action.DoubleClick(element).Build().Perform();
                }
                catch (StaleElementReferenceException)
                {
                    Thread.Sleep(1000);
                    var element = FindElementWaitUntilClickable(ElementLocator, ProcessWaitParam(wait_Seconds));
                    Action.DoubleClick(element).Build().Perform();

                }
                catch (ElementClickInterceptedException)
                {
                    waitForPageLoad();

                    var element = FindElementWaitUntilClickable(ElementLocator, ProcessWaitParam(wait_Seconds));
                    Action.DoubleClick(element).Build().Perform();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                Functions.handleFailure($"Locator: {ElementLocator}", ex, optional);
                return false;
            }
            return true;
        }

        public bool GetIsDisabled(By elementLocator)
        {
            var element = FindElementWaitUntilVisible(elementLocator);

            return element.Displayed;
        }

        public string GetAttribute(By ElementLocator, string attributeName)
        {
            return FindElementWaitUntilClickable(ElementLocator).GetAttribute(attributeName);
        }

        public IWebElement FindElementWaitUntilVisible(By by, int? wait_Seconds = null)
        {
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            IWebElement target;

            try
            {
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
            }
            catch (StaleElementReferenceException)
            {
                Thread.Sleep(5000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
            }
            catch (ElementClickInterceptedException)
            {
                Thread.Sleep(2000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
            }

            ScrollIntoView(target);
            if(HIGHLIGHT_ON)
                highlight(target);

            target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
            return target;
        }

        public List<IWebElement> FindElementsWaitUntilVisible(By by, int? wait_Seconds = null)
        {
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            IWebElement target;

            try
            {
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
            }
            catch (StaleElementReferenceException)
            {
                Thread.Sleep(5000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
            }
            catch (ElementClickInterceptedException)
            {
                Thread.Sleep(2000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
            }

            return this.WebDriver.FindElements(by).ToList();
        }

        //Find Element - Wait until element is present (different from vissible)
        public IWebElement FindElementWaitUntilPresent(By by, int? wait_Seconds = null)
        {
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            IWebElement target;

            try
            {
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));
            }
            catch (StaleElementReferenceException)
            {
                Thread.Sleep(5000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));
            }
            catch (ElementClickInterceptedException)
            {
                Thread.Sleep(2000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));
            }

            ScrollIntoView(target);
            if (HIGHLIGHT_ON)
                highlight(target);
            target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));
            return target;
        }

        //Find Element - Wait Until Clickable
        public IWebElement FindElementWaitUntilClickable(By by, int? wait_Seconds = null)
        {
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            IWebElement target;

          
            target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));
            ScrollIntoView(target);
            target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
            ScrollIntoView(target);

            if (HIGHLIGHT_ON)
            {
                highlight(target);
            }

            try
            {
                //upon scroll and highlight to the element, the element would become stale for clicking
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
            }
            catch(Exception ex)
            {
                Log.Error($"Locator: {by}");
                throw ex;
            }

            return target;
        }
        public void WaitForElementToDisappear(By by, int? wait_Seconds = null)
        {
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));

            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(by));            
        }

        //
        //  Text Fields Actions
        //

        public void setText(By TextFieldLocator, String TextToEnter, int? wait_Seconds = null)
        {
            var textField = FindElementWaitUntilClickable(TextFieldLocator, ProcessWaitParam(wait_Seconds));
            textField.Click();
            textField.SendKeys(Keys.Control + "a");
            textField.SendKeys(Keys.Delete);
            textField.SendKeys(TextToEnter);
        }

        public string getTextFieldText(By TextFieldLocator, int? wait_Seconds = null)
        {
            var textField = FindElementWaitUntilVisible(TextFieldLocator, ProcessWaitParam(wait_Seconds));
            return textField.GetAttribute("value"); 
        }

        public void clearTextField(By TextFieldLocator, int? wait_Seconds = null)
        {
            var textField = FindElementWaitUntilVisible(TextFieldLocator, ProcessWaitParam(wait_Seconds));
            textField.SendKeys(Keys.Control + "a");
            textField.SendKeys(Keys.Delete);
        }

        // 
        // Dropdown actions 
        // 

        public void SelectMatDropdownOptionByText(By DropdownLocator, string optionDisplayText)
        {
            Click(DropdownLocator);
            Click(By.XPath($"//mat-option[descendant::*[normalize-space(text())= '{optionDisplayText}']]"));
        }
        public void SelectMatDropdownOptionContainingText(By DropdownLocator, string optionDisplayText)
        {
            Click(DropdownLocator);
            Click(By.XPath($"//mat-option[descendant::*[contains(normalize-space(text()), '{optionDisplayText}')]]"));
        }

        public void SelectMatDropdownOptionByIndex(By DropdownLocator, int LogicalIndex)
        {
            Click(DropdownLocator);
            Click(By.XPath($"//mat-option[{LogicalIndex + 1}]"));
        }

        public void SelectMatDropdownOptionByIndex(By DropdownLocator, int LogicalIndex, out string selectionDisplayName)
        {
            Click(DropdownLocator);
            try
            {
                WaitForElementToDisappear(By.XPath("//mat-option[descendant::*[normalize-space(text())= 'Searching...']]"));
            }catch(Exception)
            {
            }
            var options = FindElementsWaitUntilVisible(By.XPath($"//mat-option"));
            selectionDisplayName = string.Join("", this.WebDriver.FindElements(By.XPath($"(//mat-option)[{LogicalIndex + 1}]/descendant::*")).Select(it => it.Text.Trim()).Distinct());
            Click(By.XPath($"//mat-option[{LogicalIndex + 1}]"));
        }

        public IEnumerable<String> GetAllMatDropdownOptions(By DropdownLocator)
        {
            var dropdown = FindElementWaitUntilClickable(DropdownLocator);
            dropdown.Click();
            var options = FindElementsWaitUntilVisible(By.XPath($"//mat-option"));

            int currentOption = 1;
            foreach(var option in options)
            {
                List<string> innerText = FindElementsWaitUntilVisible(By.XPath($"(//mat-option)[{currentOption}]/descendant::*")).Select(it => it.Text.Trim()).Distinct().ToList();
                currentOption++;
                yield return string.Join("", innerText);
            }
        }

        //
        // Radio Button
        //

        public bool IsRadioButtonSelected(By RadioButtonLocator)
        {
            var radioButton =FindElementWaitUntilPresent(RadioButtonLocator);

            return radioButton.Selected;
        }

        //
        // Checkbox
        //

        public void SetMattCheckboxState(By MattCheckBoxLocator, bool state)
        {
            var mattCheckBox = FindElementWaitUntilVisible(MattCheckBoxLocator);

            while (GetCheckboxState(By.Id(mattCheckBox.GetAttribute("id") + "-input")) != state)
            {
                mattCheckBox.Click();
            }
        }

        public bool GetMattCheckboxState(By MattCheckBoxLocator)
        {
            var mattCheckBox = FindElementWaitUntilClickable(MattCheckBoxLocator);
            return GetCheckboxState(By.Id(mattCheckBox.GetAttribute("id") + "-input"));
        }

        public bool GetCheckboxState(By CheckBoxInputLocator)
        {
            var CheckboxInput = FindElementWaitUntilVisible(CheckBoxInputLocator);

            return CheckboxInput.Selected;
        }


        // Scroll

        public void ScrollIntoView(IWebElement element)
        {
            JSExecutor.execute($"arguments[0].scrollIntoView();", element);
        }

        public void ScrollToBottom()
        {
            new Actions(this.WebDriver).SendKeys(Keys.End).Build().Perform();  
        }

        public void ScrollToTop()
        {
            new Actions(this.WebDriver).SendKeys(Keys.Home).Build().Perform();
        }

        //
        //  Javascript
        //

        private void highlight(IWebElement target)
        {
            JSExecutor.highlight(target);
            Thread.Sleep(200);
            try
            {
                JSExecutor.highlight(target, 0);
            }
            catch
            {
                //do nothing
            }
        }

        //spinner
        public void WaitForSpinnerToDisappear()
        {
            WebDriverWait waitAppear = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(5));
            WebDriverWait waitDisappear = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(DEFAULT_WAIT_SECONDS));

            By spinnerBy = By.XPath("//bh-mat-spinner-overlay");

            //wait until visible, need try in case spinner doesn't appear
            try
            {
                waitAppear.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(spinnerBy));
            }
            catch { return; }

            //at this point, spinner appeared, wait until invisible
            waitDisappear.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(spinnerBy));

        }

        public IEnumerable<Dictionary<String, String>> parseUITable(string datatableXpath)
        {
            FindElementWaitUntilPresent(By.XPath(datatableXpath));
            List<String> columnNames = this.WebDriver.FindElements(By.XPath(datatableXpath + "//datatable-header-cell//span[contains(@class,'datatable-header-cell-label')]")).Select(element => element.Text).ToList<String>();

            int rowCount = this.WebDriver.FindElements(By.XPath(datatableXpath + "//datatable-body-row")).Count;
            for (int rowIndex = 1; rowIndex <= rowCount; rowIndex++)
            {
                var rowDict = new Dictionary<String, String>();

                for (int i = 0; i < columnNames.Count(); i++)
                {
                    // String cellText = string.Join("", cells[i].FindElements(By.XPath("/descendant::*"))
                    String cellText = string.Join("", this.WebDriver
                                                      .FindElements(By.XPath($"(({datatableXpath} //datatable-body-row)[{rowIndex}] //datatable-body-cell)[{i + 1}]/descendant::*"))
                                                      .Select(child => child.Text).Distinct());

                    rowDict.Add(columnNames[i], cellText.Trim());
                }
                yield return rowDict;
            }
        }
    }
}
