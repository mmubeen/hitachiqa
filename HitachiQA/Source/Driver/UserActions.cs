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
using By = HitachiQA.Driver.By;
using HtmlAgilityPack;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Runtime.CompilerServices;
using DocumentFormat.OpenXml.Bibliography;
using Polly;
using AngleSharp.Text;

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

            if (wait != null) {
                DEFAULT_WAIT_SECONDS = int.Parse(wait.Value);
            }
            if (highlight != null) {
                HIGHLIGHT_ON = bool.Parse(highlight.Value);
            }
            if (loadingXPath != null) {
                LOADING_SCREEN_XPATH = loadingXPath.Value;
            }
        }

        public int DEFAULT_WAIT_SECONDS = 30;

        public bool HIGHLIGHT_ON = false;

        //Most applications have some sort of loading screen, please allow this variable to hold the that locator. please set this xpath in your .env.json file
        private readonly String LOADING_SCREEN_XPATH = "";

        public int ProcessWaitParam(int? wait) => (int)(wait == null ? DEFAULT_WAIT_SECONDS : wait);


        public void waitForPageLoad(By? iframe=null)
        {
            if (!string.IsNullOrWhiteSpace(LOADING_SCREEN_XPATH))
            {
                By locator = By.XPath(LOADING_SCREEN_XPATH,  iframe);
                //this is optional
                try
                {
                    WaitForElementToDisappear(locator, 120);
                }
                catch (Exception)
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
            var textField = FindElementWaitUntilPresent(ElementLocator, ProcessWaitParam(wait_Seconds));
            return textField.Text.Trim();
        }

        public bool Click(By ElementLocator, int? wait_Seconds = null, bool optional = false)
        {
            try
            {
                try
                {
                    WaitForTransaction();
                    waitForPageLoad(ElementLocator.IFrameLocator);
                    FindElementWaitUntilClickable(ElementLocator, ProcessWaitParam(wait_Seconds)).Click();
                }
                catch (StaleElementReferenceException)
                {
                    Thread.Sleep(1000);
                    WaitForTransaction();
                    waitForPageLoad(ElementLocator.IFrameLocator);
                    FindElementWaitUntilClickable(ElementLocator, ProcessWaitParam(wait_Seconds)).Click();

                }
                catch (ElementClickInterceptedException)
                {
                    Thread.Sleep(1000);
                    WaitForTransaction();
                    waitForPageLoad(ElementLocator.IFrameLocator);
                    FindElementWaitUntilClickable(ElementLocator, ProcessWaitParam(wait_Seconds)).Click();
                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                if(optional)
                {
                    return false;
                }
                throw new Exception($"Locator: {ElementLocator}", ex);
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
                    var element = FindElementWaitUntilPresent(ElementLocator, ProcessWaitParam(wait_Seconds));
                    Action.MoveToElement(element).DoubleClick(element).Build().Perform();
                }
                catch (StaleElementReferenceException)
                {
                    Thread.Sleep(1000);
                    var element = FindElementWaitUntilPresent(ElementLocator, ProcessWaitParam(wait_Seconds));
                    Action.DoubleClick(element).Build().Perform();

                }
                catch (ElementClickInterceptedException)
                {
                    WaitForTransaction();
                    waitForPageLoad(ElementLocator.IFrameLocator);

                    var element = FindElementWaitUntilPresent(ElementLocator, ProcessWaitParam(wait_Seconds));
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
            var element = FindElementWaitUntilPresent(elementLocator);

            return !element.Enabled;
        }
        public bool GetIsDisplayed(By elementLocator)
        {
            var element = FindElementWaitUntilPresent(elementLocator);

            return element.Displayed;
        }

        public string GetAttribute(By ElementLocator, string attributeName)
        {
            return FindElementWaitUntilClickable(ElementLocator).GetAttribute(attributeName);
        }

        private void switchToIFrame(By by)
        {
            this.WebDriver.SwitchTo().DefaultContent();

            if (by.IFrameLocator != null)
            {
                if(by.IFrameLocator.IFrameLocator!=null)
                {
                    switchToIFrame(by.IFrameLocator);
                }
                var frameElement = this.FindElementWaitUntilPresent(by.IFrameLocator.Locator);
                this.WebDriver.SwitchTo().Frame(frameElement);
            }

        }
        //Find Element - Wait until element is present (different from vissible)
        public IWebElement FindElementWaitUntilPresent(By by, int? wait_Seconds = null, bool optional=false)
        {
            this.switchToIFrame(by);
            var locator = by.Locator;
            return this.FindElementWaitUntilPresent(locator, wait_Seconds, optional);

        }

        public IWebElement FindElementWaitUntilPresent(OpenQA.Selenium.By locator, int? wait_Seconds = null, bool optional=false)
        {
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            IWebElement target;
            if(optional)
            {
                wait.IgnoreExceptionTypes(new []{typeof(Exception)});
            }

            try
            {
                 target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));
                
            }
            catch (StaleElementReferenceException)
            {
                Thread.Sleep(5000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));
            }
            catch (ElementClickInterceptedException)
            {
                Thread.Sleep(2000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));
            }

            ScrollIntoView(target);
            Thread.Sleep(200);
            if (HIGHLIGHT_ON)
                highlight(target);
            target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));
            return target;
        }

        public IWebElement FindElementWaitUntilVisible(By by, int? wait_Seconds = null)
        {
            switchToIFrame(by);
            var locator = by.Locator;
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));

            IWebElement target;

            try
            {
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
            }
            catch (StaleElementReferenceException)
            {
                Thread.Sleep(5000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
            }
            catch (ElementClickInterceptedException)
            {
                Thread.Sleep(2000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
            }

            ScrollIntoView(target);
            if (HIGHLIGHT_ON)
                highlight(target);

            target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
            return target;
        }

        public List<IWebElement> FindElementsWaitUntilVisible(By by, int? wait_Seconds = null)
        {
            switchToIFrame(by);
            var locator = by.Locator;
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            IWebElement target;

            try
            {
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
            }
            catch (StaleElementReferenceException)
            {
                Thread.Sleep(5000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
            }
            catch (ElementClickInterceptedException)
            {
                Thread.Sleep(2000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
            }

            return this.WebDriver.FindElements(locator).ToList();
        }
        public List<IWebElement> FindElementsWaitUntilPresent(By by, int? wait_Seconds = null)
        {
            switchToIFrame(by);
            var locator = by.Locator;
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            //wait.IgnoreExceptionTypes(typeof(Exception));
            IWebElement target;

            try
            {
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));
            }
            catch (StaleElementReferenceException)
            {
                Thread.Sleep(5000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));
            }
            catch (ElementClickInterceptedException)
            {
                Thread.Sleep(2000);

                //retry finding the element
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));
            }

            return this.WebDriver.FindElements(locator).ToList();
        }

        public void Hover(By by, int? wait_Seconds = null, bool optional=false)
        {
            switchToIFrame(by);
            var locator = by.Locator;
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            IWebElement target;
            target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));
            ScrollIntoView(target);
            var action = new Actions(this.WebDriver);
            try
            {
                action.MoveToElement(target).Build().Perform();
            }
            catch(Exception)
            {
                if(!optional)
                {
                    throw;
                }
            }
        }

        //Find Element - Wait Until Clickable
        public IWebElement FindElementWaitUntilClickable(By by, int? wait_Seconds = null)
        {
            switchToIFrame(by);
            var locator = by.Locator;
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            IWebElement target;

            try
            {
                Hover(by, wait_Seconds, true);
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));

            }
            catch (StaleElementReferenceException)
            {
                Thread.Sleep(2000);
                Hover(by, wait_Seconds, true);
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
            }
            catch (ElementClickInterceptedException)
            {
                Thread.Sleep(2000);
                Hover(by, wait_Seconds, true);
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
                
            }

            if (HIGHLIGHT_ON)
            {
                highlight(target);
            }

            try
            {
                //upon scroll and highlight to the element, the element would become stale for clicking
                target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
            }
            catch (Exception)
            {
                Log.Error($"Locator: {locator}");
                throw;
            }

            return target;
        }
        public void WaitForElementToDisappear(By by, int? wait_Seconds = null)
        {
            this.switchToIFrame(by);
            var locator = by.Locator;

            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));

            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(locator));
        }

        //
        //  Text Fields Actions
        //

        public void setText(By TextFieldLocator, String TextToEnter, int? wait_Seconds = null)
        {
            Actions Action = new Actions(this.WebDriver);
            var textField = FindElementWaitUntilClickable(TextFieldLocator, ProcessWaitParam(wait_Seconds));
            Action.MoveToElement(textField).Click(textField).Build().Perform();
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
            } catch (Exception)
            {
            }
            var options = FindElementsWaitUntilVisible(By.XPath($"//mat-option"));
            selectionDisplayName = string.Join("", this.WebDriver.FindElements(By.XPath($"(//mat-option)[{LogicalIndex + 1}]/descendant::*").Locator).Select(it => it.Text.Trim()).Distinct());
            Click(By.XPath($"//mat-option[{LogicalIndex + 1}]"));
        }

        public IEnumerable<String> GetAllMatDropdownOptions(By DropdownLocator)
        {
            var dropdown = FindElementWaitUntilClickable(DropdownLocator);
            dropdown.Click();
            var options = FindElementsWaitUntilVisible(By.XPath($"//mat-option"));

            int currentOption = 1;
            foreach (var option in options)
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
            var radioButton = FindElementWaitUntilPresent(RadioButtonLocator);

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
            var CheckboxInput = FindElementWaitUntilPresent(CheckBoxInputLocator);

            return CheckboxInput.Selected;
        }


        // Scroll

        public void ScrollIntoView(IWebElement element)
        {
            JSExecutor.execute($"arguments[0].scrollIntoViewIfNeeded();", element);
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

            var spinnerLocator = By.XPath("//bh-mat-spinner-overlay").Locator;

            //wait until visible, need try in case spinner doesn't appear
            try
            {
                waitAppear.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(spinnerLocator));
            }
            catch { return; }

            //at this point, spinner appeared, wait until invisible
            waitDisappear.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(spinnerLocator));

        }

        public IEnumerable<Dictionary<String, String>> parseUITable(By datatable)
        {
            var tableElement = FindElementWaitUntilPresent(datatable);
            //Mat UI bootstrap table
            if (this.ElementExists(By.XPath(datatable.Locator.Criteria + "//datatable-header-cell", datatable.IFrameLocator)))
            {

                var datatableXpath = datatable.Locator.Criteria;
                List<String> columnNames = this.WebDriver.FindElements(By.XPath(datatableXpath + "//datatable-header-cell//span[contains(@class,'datatable-header-cell-label')]", datatable.IFrameLocator).Locator).Select(element => element.Text).ToList<String>();

                int rowCount = this.WebDriver.FindElements(By.XPath(datatableXpath + "//datatable-body-row", datatable.IFrameLocator).Locator).Count;
                for (int rowIndex = 1; rowIndex <= rowCount; rowIndex++)
                {
                    var rowDict = new Dictionary<String, String>();

                    for (int i = 0; i < columnNames.Count(); i++)
                    {
                        // String cellText = string.Join("", cells[i].FindElements(By.XPath("/descendant::*"))
                        String cellText = string.Join("", this.WebDriver
                                                          .FindElements(By.XPath($"(({datatableXpath} //datatable-body-row)[{rowIndex}] //datatable-body-cell)[{i + 1}]/descendant::*", datatable.IFrameLocator).Locator)
                                                          .Select(child => child.Text).Distinct());

                        rowDict.Add(columnNames[i], cellText.Trim());
                    }
                    yield return rowDict;
                }
            }
            else if (this.ElementExists(By.XPath(datatable.Locator.Criteria + "//th", datatable.IFrameLocator)))
            {
                var headers = this.GetUITableHeaders(datatable);

               
                var rowsXPath = "//tbody//tr[not(.//tr)]";
                var dataXPath = "//td";
                
              
                if(this.FindElementsWaitUntilPresent(By.XPath($"({datatable.Locator.Criteria} {rowsXPath})[1] {dataXPath}", datatable.IFrameLocator)).Count==1 && headers.Count!=1 )
                {
                    yield return headers.ToDictionary(it=> !string.IsNullOrEmpty(it.Value) ? it.Value : it.Key.ToString(), it=> "");
                }
                else
                {
                    var tableDoc = new HtmlDocument();
                    tableDoc.LoadHtml(tableElement.GetAttribute("innerHTML"));
                    var rows = tableDoc.DocumentNode.SelectNodes(rowsXPath);
                    var rowIndex = 0;
                    foreach(var rowNode in rows)
                    {
                        var rowDict = new Dictionary<string, string>();
                        rowDict.Add("index", rowIndex.ToString());
                        var rowHTML = rowNode.InnerHtml;
                        var rowDoc = new HtmlDocument();
                        rowDoc.LoadHtml(rowHTML);
                        var row = rowDoc.DocumentNode;
                        var cells = row.SelectNodes(dataXPath);
                        var cellIndex = 0;
                        foreach(var cell in cells)
                        {
                            var header = headers[cellIndex] ?? throw new NullReferenceException();
                            if(string.IsNullOrWhiteSpace(header))
                            {
                                header = cellIndex.ToString();
                            }
                            var children = cell.ChildNodes.Select(it=> it.InnerText.Trim()).ToList();
                            children.Add(cell.InnerText.Trim());
                            var cellText = string.Join("", children.Distinct())?? "";
                            cellText = System.Web.HttpUtility.HtmlDecode(cellText);

                            rowDict.Add(header, cellText);
                            cellIndex++;
                        }
                        rowIndex++;
                        yield return rowDict;
                    }
                }
               
              
            }
            else
            {
                throw new NotImplementedException(datatable.Locator.Criteria);
            }
        }
        public Dictionary<int, string?> GetUITableHeaders(By table)
        {
            Dictionary<int, string?> result = new Dictionary<int, string?>();

            var tableElement = FindElementWaitUntilPresent(table);

            var tableDoc = new HtmlDocument();
            tableDoc.LoadHtml(tableElement.GetAttribute("innerHTML"));
            var headersXPath = "//th[text()]/..//th";
            var headers = tableDoc.DocumentNode.SelectNodes(headersXPath);
            if(headers==null || headers.Count==0)
            {
               headers = tableDoc.DocumentNode.SelectNodes("//th");
            }
            var index = 0;
            foreach (var header in headers)
            {
                var children = header.ChildNodes.Select(it=> it.InnerText.Trim()).ToList();
                children.Add(header.InnerText.Trim());
                
                var cellText = string.Join("", children.Distinct())?? "";
                cellText = System.Web.HttpUtility.HtmlDecode(cellText);

                result.Add(index, cellText);
                index++;
            }
            return result;

        }


        private const string HORIZONTAL_SCROLL_BAR = "//div[@class='ag-body-horizontal-scroll' or contains(@class, 'ScrollbarLayout_main Scrollbar')]";
        // private const string HORIZONTAL_SCROLL_BAR = "//div[@class='ag-body-horizontal-scroll' or contains(@class, 'ScrollbarLayout_main Scrollbar')] //div[@ref='eViewport']";

        private object? GRID_SCROLL_JS_EXEC(By gridLocator, string command)
        {
            var scrollBarLoc = By.XPath($"{gridLocator.Locator.Criteria} {HORIZONTAL_SCROLL_BAR}");
            if(!ElementExists(scrollBarLoc, out var element))
            {
                return null;
            }
            return JSExecutor.execute(command, element?? throw new NullReferenceException("element"));
        }

        private object? GRID_SCROLL_LEFT_COMMAND(By gridLocator) => GRID_SCROLL_JS_EXEC(gridLocator, "arguments[0].scrollBy(-400, 0);");
        private object? GRID_SCROLL_ALL_RIGHT_COMMAND(By gridLocator) => GRID_SCROLL_JS_EXEC(gridLocator, "arguments[0].scrollBy(12000, 0);");
        private object? GRID_QUERY_HOW_MUCH_UNTIL_RESET(By gridLocator) => GRID_SCROLL_JS_EXEC(gridLocator, "return arguments[0].scrollLeft;");

       


        public Dictionary<int, string?> GetGridHeaders(By by)
        {


            this.WaitForTransaction();

            string gridCellXPath = $"//*[@role='columnheader' and @aria-colindex] | //*[@data-dyn-columnname]/*[@title or @data-dyn-qtip-title]";

            Dictionary<int, string?> result = new Dictionary<int, string?>();


            GRID_SCROLL_ALL_RIGHT_COMMAND(by);
            do
            {
                this.FindElementWaitUntilPresent(By.XPath(by.Locator.Criteria+gridCellXPath));
                var gridElement = this.FindElementWaitUntilPresent(by);
                
                var gridDoc = new HtmlDocument();
                gridDoc.LoadHtml(gridElement.GetAttribute("innerHTML"));

                var headers = gridDoc.DocumentNode.SelectNodes(gridCellXPath);
                var iteration = 0;
                foreach (var header in headers)
                {
                    
                    var index = int.Parse(header.GetAttributeValue("aria-colindex", "-1"));
                    string? displayText = header.GetAttributeValue("title", null);
                    if(index== -1)
                    {
                        displayText = header.InnerText;
                    }
                    //PCF grids have title in a child node
                    if (displayText == null)
                    {
                        var rowDoc = new HtmlDocument();
                        rowDoc.LoadHtml(header.InnerHtml);

                        displayText = rowDoc.DocumentNode.SelectSingleNode("//*[@title or @data-dyn-qtip-title]")?.InnerText;
                        displayText = System.Web.HttpUtility.HtmlDecode(displayText??"");
                    }

                    iteration++;

                    if (index!=-1 && !result.ContainsKey(index))
                    {
                        result.Add(index, displayText);
                    }
                    else if(index==-1)
                    {
                        result.Add(iteration, displayText);
                    }
                }

                GRID_SCROLL_LEFT_COMMAND(by);
            }
            while (GRID_QUERY_HOW_MUCH_UNTIL_RESET(by) is var queryResult && queryResult!=null && Convert.ToDouble(queryResult) != 0 );

            return result;
        }

        public List<Dictionary<string, string?>> GetGridItems(By by)
        {
            this.WaitForTransaction();
           

            var headers = this.GetGridHeaders(by);
            var results = new List<Dictionary<string, string?>>();

            var rowXPath = $"//*[@role='row' and (@aria-rowindex or descendant::*[@aria-colindex]) ]";

            GRID_SCROLL_ALL_RIGHT_COMMAND(by);
            do
            {
                var gridElement = this.FindElementWaitUntilPresent(by);
                var gridDoc = new HtmlDocument();
                gridDoc.LoadHtml(gridElement.GetAttribute("innerHTML"));

                var rows = gridDoc.DocumentNode.SelectNodes(rowXPath);
                //remove header (1st row)
                var headerNode = rows.FirstOrDefault(it=> it.GetAttributeValue<int>("aria-rowindex", -1)==1);
                headerNode.NullGuard();
                rows.Remove(headerNode);
                
                foreach(var rowNode in rows)
                {
                    var rowHTML = rowNode.InnerHtml; 
                    var rowDoc = new HtmlDocument();
                    rowDoc.LoadHtml(rowHTML);
                    var row = rowDoc.DocumentNode;
                    var index = rowNode.GetAttributeValue<string?>("aria-rowindex", null);
                    index.NullGuard();
                    //because we ignore the header
                    index = (int.Parse(index) - 1).ToString();

                    var rowDict = results.GetDictionaryByIndex(index);
                    if (rowDict == null)
                    {
                        rowDict = new Dictionary<string, string?>();
                        rowDict.Add("index", index);
                        rowDict.Add("id", rowNode.GetAttributeValue<string>("row-id", ""));
                        results.Add(rowDict);
                    }

                    foreach (var header in headers)
                    {
                        var headerIndex = header.Key;
                        var headerName = header.Value ?? header.Key.ToString();
                        string? cellValue = null;

                        var xpaths_and_atts = new Dictionary<string, string>()
                        {
                            {$"//*[@aria-colindex='{headerIndex}' and @col-id]  //*[@aria-label]", "aria-label" },
                            {$"//*[@aria-colindex='{headerIndex}' and @title]", "title" },
                            {$"//*[@aria-label='{headerName}' and @title]", "title" }

                        };

                        foreach(var item in xpaths_and_atts)
                        {
                            var xpath = item.Key;
                            var attr = item.Value;
                            var cellNodes = row.SelectNodes(xpath);
                            HtmlNode? cellNode = null;
                            if(cellNodes?.Count > 1)
                            {
                                cellNode = cellNodes.FirstOrDefault(it => it.GetAttributeValue<string?>("type", null) == "button");

                                if(cellNode == null)
                                    throw new NotImplementedException($"Found 2 cells with @aria-label attribute using the following xpath: {by.Locator.Criteria+rowXPath+xpath}");
                            }
                            else if (cellNodes?.Count == 1){
                                cellNode = cellNodes[0];
                            }


                            if (cellNode != null)
                            {
                                cellValue = cellNode.GetAttributeValue(attr, "");
                                break;
                            }

                        }

                     
                        if (rowDict.GetValueOrDefault(headerName) == null || !string.IsNullOrWhiteSpace(cellValue))
                        {
                            cellValue = System.Web.HttpUtility.HtmlDecode(cellValue??"");
                            rowDict[headerName] = cellValue;
                        }
                        
                    }



                }

                GRID_SCROLL_LEFT_COMMAND(by);
            }
            while (Convert.ToDouble(GRID_QUERY_HOW_MUCH_UNTIL_RESET(by)) != 0);


            return results;
        }

        public void OpenGridRecord(By by, string columnName, string value)
        {
            this.WaitForTransaction();
            var gridItems = this.GetGridItems(by);

            var matchingRow = gridItems.FirstOrDefault(row=> row.TryGetValue(columnName, out var colVal) && colVal == value);

            if(matchingRow==null)
            {
                throw new NotFoundException($"Couldn't find row matching {columnName}={value}");
            }

            var index = matchingRow["index"];

            var checkBoxLocDoubleClick = By.XPath(by.Locator.Criteria + $"//*[@role='row' and @aria-rowindex={int.Parse(index)+1} and descendant::*[@aria-colindex=1] ]//i[@data-icon-name]");
            if(ElementExists(checkBoxLocDoubleClick))
            {
                this.DoubleClick(checkBoxLocDoubleClick);
            }
            else{
                var checkBoxLoc = By.XPath(by.Locator.Criteria + $"//*[@role='row' and @aria-rowindex={int.Parse(index)+1}]//*[@title='Select or unselect row']");
                this.Click(checkBoxLoc);
                var firstCol = By.XPath(by.Locator.Criteria + $"//*[@role='row' and @aria-rowindex={int.Parse(index)+1}]//input[@aria-label]");
                this.Click(firstCol);

            }

            //var EditButtonLoc = By.XPath("((//div[@id='mainContent'] //*[contains(@data-id, 'Command')])[1] | //*[@data-id='OverflowFlyout']) //button[*//text()='Edit']");

            //this.FindElementWaitUntilClickable(by).Click();
            this.WaitForTransaction();

        }
        public void SelectGridRecord(By by, string columnName, string value)
        {
            var gridItems = this.GetGridItems(by);

            var matchingRow = gridItems.FirstOrDefault(row => row.TryGetValue(columnName, out var colVal) && colVal == value);

            if (matchingRow == null)
            {
                throw new NotFoundException($"Couldn't find row matching {columnName}={value}");
            }

            var index = matchingRow["index"];

            var checkBoxLoc = By.XPath(by.Locator.Criteria + $"//*[@role='row' and @aria-rowindex={int.Parse(index) + 1} and descendant::*[@aria-colindex=1] ]//i[contains(@data-icon-name, 'Check')]/..");
            this.Click(checkBoxLoc);
        }
        public void SelectAllGridRecords(By by)
        {
            var checkBoxLoc = By.XPath(by.Locator.Criteria + $"//*[@role='row' and @aria-rowindex=1 and descendant::*[@aria-colindex=1] ]//i[contains(@data-icon-name, 'Check')]/..");
            this.Click(checkBoxLoc);
        }
        public void SortGridColumn(By by, string columnName, string filterByString = "", bool descendingSort = false, string comparisonOperation = "")
        {
            this.WaitForTransaction();

            string gridCellXPath = $"//*[@role='columnheader']";
            var columnIndex=-1;
            GRID_SCROLL_ALL_RIGHT_COMMAND(by);
            do
            {
                var gridElement = this.FindElementWaitUntilPresent(by);
                var gridDoc = new HtmlDocument();
                gridDoc.LoadHtml(gridElement.GetAttribute("innerHTML"));

                var headers = gridDoc.DocumentNode.SelectNodes(gridCellXPath);

                foreach (var header in headers)
                {
                    var index = int.Parse(header.GetAttributeValue("aria-colindex", null));
                    var displayText = header.GetAttributeValue("title", null);

                    //PCF grids have title in a child node
                    if (displayText == null)
                    {
                        var rowDoc = new HtmlDocument();
                        rowDoc.LoadHtml(header.InnerHtml);

                        displayText = rowDoc.DocumentNode.SelectSingleNode("//*[@title]")?.InnerText;
                        displayText = System.Web.HttpUtility.HtmlDecode(displayText??"");
                    }

                    if(columnName==displayText)
                    {
                        columnIndex = index;
                    }
                    
                }

                GRID_SCROLL_LEFT_COMMAND(by);
            }
            while (Convert.ToDouble(GRID_QUERY_HOW_MUCH_UNTIL_RESET(by)) != 0 && columnIndex==-1);

            var columnLocator = By.XPath(by.Locator.Criteria+$"//*[@aria-colindex='{columnIndex}' and contains(@class,'header')]//*[@role='button']", by.IFrameLocator);
            var ascendingButton = By.XPath("//ul[contains(@class,'ContextualMenu-list is-open')]//li//button[@name='Sort A to Z']", by.IFrameLocator);                        
            var descendingButton = By.XPath("//ul[contains(@class,'ContextualMenu-list is-open')]//li//button[@name='Sort Z to A']", by.IFrameLocator);
            var filterByButton = By.XPath("//ul[contains(@class,'ContextualMenu-list is-open')]//li//button[@name='Filter by']", by.IFrameLocator);
            var comparisorOperatorDropdown = By.XPath("//*[contains(@class,'calloutMain')] //div[contains(@id,'Dropdown')]", by.IFrameLocator);
            var comparisonOperatorButton = By.XPath($"//*[contains(@class,'calloutMain')]//button[.//*[text()='{(comparisonOperation==""?"Equals": comparisonOperation)}']]");
            var applyButon = By.XPath("//button[.//*[text()='Apply']]", by.IFrameLocator);
            
            Click(columnLocator);
            Click(filterByButton);
            Click(comparisorOperatorDropdown);
            Click(comparisonOperatorButton);

            this.SetFieldValue(By.XPath("//*[contains(@class,'calloutMain')]//*[contains(@class,'operatorsDropdownContainer')]/following-sibling::*[1]", by.IFrameLocator), filterByString);

            Click(applyButon);


            Click(columnLocator);
            if(descendingSort)
                Click(descendingButton);
            else
                Click(ascendingButton);

        }
        public static Dictionary<string, string> KnownXPaths = new Dictionary<string, string> {
                { "//select[contains(@data-id, 'option-set-select')]", "dropdown" },
                { "//input[@type='text' and contains(@data-id, 'text-box-text')]", "textfield" },
                { "//input[@type='text' and contains(@data-id, 'text-input')]", "textfield" },
                { "//input[@type='text' and contains(@data-id, 'quickFind_text')]", "textfield" },
                { "//textarea[@type='text' and @aria-autocomplete='list'] ", "textfield_autocomplete" },
                { "//input[@type='text' and contains(@data-id, 'textInputBox_with_filter')]", "lookup" },
                { "//*[@role='link' and contains(@id, 'selected_tag')]", "lookup_with_selection" },
                { "//*[@role='switch']", "switch" },
                { "//input[@type='text' and following-sibling::*[contains(@data-dyn-bind, 'Lookup')]]", "lookup_with_table" },
                { "//following-sibling::*/select", "dropdown"},
                { "//input[@type='checkbox']", "checkbox" },
                { "//input[contains(@class, 'editable-lookup') and following-sibling::*[.//*[@class='fa fa-search']]]", "lookup_with_dialog"},
                { "//td[@data-hslcolumnname][.//input[@type='text'] and .//input[@type='submit']]", "effective_grid_lookup"},
                { "//textarea[not(@type) and not(@aria-autocomplete)]", "textfield"},
                { "//input[@data-role='numerictextbox']", "numerictextbox"},
                { "//input[@type='text' and contains(@data-bind,'currency')]", "textfield"},
                { "//div[@role='combobox']", "combobox"},
                { "//*[contains(@class,'enumValuesDropdown')]//*[@role='combobox']", "enum_combobox"},
                { "//self::*[.//*[contains(@id, 'DatePicker')]]", "datepicker"},
                { "//input[@role='combobox' and not(contains(@id, 'DatePicker'))]", "input_with_combobox"},
                { "//div[contains(@class,'ms-TextField is-disabled')]//button", "textfieldreadonly"},
                { "//input[@data-role='dropdownlist']/../..", "dropdown_listbox" },
                { "//select", "dropdown"},
                { "//input", "textfield"}
            };

        public void SetFieldValue(By by, string value) {
            this.waitForPageLoad(by.IFrameLocator);
            this.WaitForTransaction();

            var fieldElement = this.FindElementWaitUntilPresent(by);
            var fieldDoc = new HtmlDocument();
            fieldDoc.LoadHtml(fieldElement.GetAttribute("outerHTML"));


            KeyValuePair<string, string> matchingPair = FindKnownXPathMatchingPair(fieldDoc, out HtmlNode? node, by); 
            matchingPair.NullGuard();

            var autoGeneratedLocator = GetAutoGeneratedLocator(by, matchingPair);

            try
            {


                switch (matchingPair.Value)
                {
                    case "dropdown":
                        this.SelectDropdownValue(autoGeneratedLocator, value);
                        break;
                    case "textfield_autocomplete":
                        this.setText(autoGeneratedLocator, value);
                        var autoCompleteItemLoc = By.XPath("//section[contains(@id, 'fieldControl')] //li");
                        this.FindElementWaitUntilClickable(autoCompleteItemLoc).Click();
                        break;
                    case "textfield":
                        this.setText(autoGeneratedLocator, value);
                        break;
                    case "lookup_with_selection":
                        new Actions(this.WebDriver).MoveToElement(this.FindElementWaitUntilPresent(autoGeneratedLocator)).Perform();
                        var removeButtonLocator = By.XPath(autoGeneratedLocator.Locator.Criteria + "/following-sibling::button", by.IFrameLocator);
                        this.FindElementWaitUntilPresent(removeButtonLocator).Click();
                        var lookupPath = KnownXPaths.First(it => it.Value == "lookup").Key;
                        autoGeneratedLocator = By.XPath($"{by.Locator.Criteria} {lookupPath}", by.IFrameLocator);
                        performLookupValueSet(autoGeneratedLocator, value);
                        break;
                    case "lookup":
                        performLookupValueSet(autoGeneratedLocator, value);
                        break;
                    case "switch":
                        var switchVal = parseStrIntoBool(value);
                        var element = FindElementWaitUntilClickable(autoGeneratedLocator);

                        if (bool.Parse(element.GetAttribute("aria-checked")) != switchVal)
                            element.Click();
                        break;
                    case "lookup_with_table":
                        this.setText(autoGeneratedLocator, value);
                        //index=1 is the header and index=2 is the first row
                        this.Click(By.XPath("//form[contains(@class, 'lookup-popup active-form')]//*[@role='row' and @aria-rowindex='2']"));
                        break;
                    case "lookup_with_dialog":
                        this.Click(By.XPath(autoGeneratedLocator.Locator.Criteria+"/following-sibling::*//*[@class='fa fa-search']", autoGeneratedLocator.IFrameLocator));
                        this.SetFieldValue(By.XPath("//*[@id='lookupDialogRoot'] //input[contains(@id, 'lookupDialogLookup')]"), value);
                        this.Click(By.XPath("//*[@id='lookupDialogRoot'] //button[@aria-label ='Add']"));
                       break;
                    case "checkbox":
                        var checkboxVal = parseStrIntoBool(value);
                        if(checkboxVal != this.GetCheckboxState(autoGeneratedLocator))
                        {
                            try{
                                Click(autoGeneratedLocator);
                            }
                            catch(ElementClickInterceptedException)
                            {
                                Click(By.XPath(autoGeneratedLocator.Locator.Criteria+"/..", autoGeneratedLocator.IFrameLocator));
                            }
                            
                        }
                        break;
                    case "effective_grid_lookup":
                    
                        this.Click(By.XPath(autoGeneratedLocator.Locator.Criteria+"//input[@type='submit']", autoGeneratedLocator.IFrameLocator));
                        var lookupIframe = By.XPath("//iframe[contains(@id, 'hisol-dialog')]");
                        this.setText(By.XPath("//*[@class='lookupDialogContainer']//input[@type='text']", lookupIframe), value);
                        this.Click(By.XPath("//*[contains(@class,'crmSearchIcon')]", lookupIframe));
                        this.Click(By.XPath("(//tr//input[@type='checkbox'])[1]", lookupIframe));
                        this.Click(By.XPath("//button[text()='Ok']", lookupIframe));
                    
                        break;
                    case "numerictextbox":
                        fieldElement.Click();
                        var numericTextBox = this.FindElementWaitUntilPresent(autoGeneratedLocator);
                        numericTextBox.Clear();
                         fieldElement.Click();
                        numericTextBox.SendKeys(value);
                        // this.JSExecutor.execute($"arguments[0].value={value}", numericTextBox);
                        break;
                    case "combobox":
                        this.Click(autoGeneratedLocator);
                        var targetOption = By.XPath($"(//button[contains(@class,'dropdownItem') and .//*[text()='{value}']]) | //*[contains(@class, 'ms-Checkbox') and @title='{value}']");
                        this.Click(targetOption);
                        break;
                    case "datepicker":

                        //
                        //Date
                        //
                        var targetDate = DateTime.Parse(value);
                        this.SetDatePickerValue(autoGeneratedLocator, targetDate);
                        //
                        //Time
                        //
                        
                        var timeLocator = By.XPath($"({by.Locator.Criteria})//input[contains(@aria-label,'Time')]", by.IFrameLocator);
                        if(this.FindElementWaitUntilPresent(timeLocator,1,true)!=null)
                        {
                            this.setText(timeLocator, targetDate.ToString("hh:mm tt"));
                        }

                        break;
                    case "dropdown_listbox":
                        this.Click(autoGeneratedLocator);
                        this.Click(By.XPath($"//div[@data-role='popup' and contains(@style,'display: block')]//li[normalize-space()='{value}']", autoGeneratedLocator.IFrameLocator));
                        break;
                    case "input_with_combobox":
                        this.setText(autoGeneratedLocator, value);
                        this.Click(By.XPath("//*[contains(@class,'suggestions')]//button", autoGeneratedLocator.IFrameLocator));
                        break;
                    case "enum_combobox":
                        this.Click(autoGeneratedLocator);
                        var option = By.XPath($"//*[contains(@class,'calloutMain')] //input[@type='checkbox' and @title='{value}']/..", autoGeneratedLocator.IFrameLocator);
                        this.Click(option);
                        break;
                    default: throw new NotImplementedException($"Method for field type {matchingPair.Value} has not been implemented");
                    
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Error setting value for \n{autoGeneratedLocator.Locator}\n matched pair {matchingPair.Value}={matchingPair.Key}", ex);
            }
            this.waitForPageLoad(autoGeneratedLocator.IFrameLocator);
            this.WaitForTransaction();
        }

        private void SetDatePickerValue(By autoGeneratedLocator, DateTime targetDate)
        {
            this.Click(By.XPath($"({autoGeneratedLocator.Locator.Criteria}//input)[1]"));
            var datePickerXPath = "//*[contains(@id, 'DatePicker-Callout')]";
            //
            //possible display fomats on currentItemButton 
            //February 2023  
            //2023
            //2020-2031
            //
            var currentItemButtonXPath = $"({datePickerXPath} //button[contains(@class,'currentItemButton') or contains(@aria-label, 'Year picker')])[last()]";
            var monthYearPicker = By.XPath($"{datePickerXPath} //*[contains(@class, 'monthPickerWrapper')]");
            var initialMonthYear = this.FindElementWaitUntilPresent(By.XPath($"{datePickerXPath}//*[contains(@class,'monthAndYear')]/span")).Text;
            
            if(!initialMonthYear.EndsWith(targetDate.Year.ToString()))
            {
                if(!this.ElementExists(monthYearPicker))
                {
                    this.Click(By.XPath(currentItemButtonXPath));
                }
                this.Click(By.XPath(currentItemButtonXPath));
                var yearRange = this.FindElementWaitUntilPresent(By.XPath(currentItemButtonXPath+"/span")).Text;
                string[] years = yearRange.Split('-');
                int startYear = int.Parse(years[0].Trim());
                int endYear = int.Parse(years[1].Trim());
                if(targetDate.Year>=startYear && targetDate.Year <= endYear)
                {
                    this.Click(By.XPath($"{datePickerXPath}//button[text()='{targetDate.Year}']"));
                }
                else
                {
                    throw new NotImplementedException("date picker for selecting year out of range needs to be implemented");
                }
                this.Click(By.XPath($"{datePickerXPath}//button[@aria-label='{targetDate.ToString("MMMM")}']"));
            }
            else if(!initialMonthYear.StartsWith(targetDate.ToString("MMMM")))
            {
                if(!this.ElementExists(monthYearPicker))
                {
                    this.Click(By.XPath(currentItemButtonXPath));
                }
                this.Click(By.XPath($"{datePickerXPath}//button[@aria-label='{targetDate.ToString("MMMM")}']"));
            }   
            
            this.Click(By.XPath($"{datePickerXPath}//td[.//*[@aria-label='{targetDate.Day}, {targetDate.ToString("MMMM")}, {targetDate.Year}']]"));

        }

        public string GetFieldValue(By by)
        {
            this.waitForPageLoad(by.IFrameLocator);
            this.WaitForTransaction();

            var fieldElement = this.FindElementWaitUntilPresent(by);
            var fieldDoc = new HtmlDocument();
            fieldDoc.LoadHtml(fieldElement.GetAttribute("outerHTML"));


            KeyValuePair<string, string> matchingPair = FindKnownXPathMatchingPair(fieldDoc, out HtmlNode? node, by);
            matchingPair.NullGuard();

            var autoGeneratedLocator = GetAutoGeneratedLocator(by, matchingPair);

            String fieldValue = null;
            switch (matchingPair.Value)
            {
                case "dropdown":
                    fieldValue = GetSelectedDropdownValue(autoGeneratedLocator);
                    break;
                case "textfield_autocomplete":
                    fieldValue = this.getTextFieldText(autoGeneratedLocator);

                    break;
                case "mat-autocomplete":
                    fieldValue = this.getTextFieldText(autoGeneratedLocator);

                    break;
                case "textfield":
                    fieldValue = this.getTextFieldText(autoGeneratedLocator);
                    break;
                case "lookup_with_selection":
                    fieldValue = this.getElementText(By.XPath(autoGeneratedLocator.Locator.Criteria+"/*",by.IFrameLocator));

                    break;
                case "lookup_with_dialog":
                    fieldValue = this.getTextFieldText(autoGeneratedLocator);
                    break;

                case "lookup":
                    fieldValue = this.getTextFieldText(autoGeneratedLocator);

                    break;
                case "switch":
                    throw new NotImplementedException("switch getter not implemented yet");
                    break;
                case "lookup_with_table":
                    fieldValue = this.getTextFieldText(autoGeneratedLocator);

                    break;
                case "mat-select":
                    var currentValContainer = By.XPath($"{autoGeneratedLocator.Locator.Criteria} //*[contains(@class, 'mat-select-value')]//*[text()]", autoGeneratedLocator.IFrameLocator);
                    fieldValue = FindElementWaitUntilVisible(currentValContainer).Text;
                    break;
                case "checkbox":
                    fieldValue = this.GetCheckboxState(autoGeneratedLocator).ToString();
                    break;
                case "datepicker":
                    fieldValue = GetDatePickerFieldValue(autoGeneratedLocator);
                    break;
                case "input_with_combobox":
                    fieldValue = this.getTextFieldText(autoGeneratedLocator);
                    break;
                case "textfieldreadonly":
                    fieldValue = this.getTextFieldText(autoGeneratedLocator);
                    break;
                default: throw new NotImplementedException($"Method for field type {matchingPair.Value} has not been implemented");
            }

            if (fieldValue == null)
            {
                throw new Exception($"error while getting field value of field located by {by.Locator}");
            }
            return fieldValue;
        }
        private string GetDatePickerFieldValue(By autoGeneratedLocator)
        {
            var date = this.getTextFieldText(By.XPath($"({autoGeneratedLocator.Locator.Criteria}//input)[1]",autoGeneratedLocator.IFrameLocator));
            var timeElement = By.XPath($"({autoGeneratedLocator.Locator.Criteria}//input)[2]",autoGeneratedLocator.IFrameLocator);
            var time = "";
            if(ElementExists(timeElement))
            {
                time = this.getTextFieldText(timeElement);
            }
            var dateTime = DateTime.Parse(date+" "+time);
            return dateTime.ToString("O");
        }
        public List<string> GetFieldOptions(By by)
        {
            this.WaitForTransaction();
            this.waitForPageLoad(by.IFrameLocator);
            var fieldElement = this.FindElementWaitUntilPresent(by);
            var fieldDoc = new HtmlDocument();
            fieldDoc.LoadHtml(fieldElement.GetAttribute("outerHTML"));


            KeyValuePair<string, string> matchingPair = FindKnownXPathMatchingPair(fieldDoc, out HtmlNode? node, by);
            matchingPair.NullGuard();

            var autoGeneratedLocator = GetAutoGeneratedLocator(by, matchingPair);

            List<string> options = null;
            switch (matchingPair.Value)
            {
                case "dropdown":
                    options = GetDropdownOptionsText(autoGeneratedLocator);
                    break;
                case "textfield_autocomplete":
                    throw new NotImplementedException($"Method for field type {matchingPair.Value} has not been implemented");
                    break;
                case "mat-autocomplete":
                    throw new NotImplementedException($"Method for field type {matchingPair.Value} has not been implemented");
                    break;
                case "lookup_with_selection":
                    throw new NotImplementedException($"Method for field type {matchingPair.Value} has not been implemented");
                    break;
                case "lookup":
                    throw new NotImplementedException($"Method for field type {matchingPair.Value} has not been implemented");
                    break;
                case "lookup_with_table":
                    throw new NotImplementedException($"Method for field type {matchingPair.Value} has not been implemented");
                    break;
                case "mat-select":
                    options = this.GetAllMatDropdownOptions(autoGeneratedLocator).ToList();
                    break;
                default: throw new NotImplementedException($"Method for field type {matchingPair.Value} has not been implemented");
            }

            if (options == null)
            {
                throw new Exception($"error while getting field value of field located by {by.Locator}");
            }
            return options;
        }
        public void OpenFieldValue(By by)
        {
            this.WaitForTransaction();
            this.waitForPageLoad(by.IFrameLocator);

            var fieldElement = this.FindElementWaitUntilPresent(by);
            var fieldDoc = new HtmlDocument();
            fieldDoc.LoadHtml(fieldElement.GetAttribute("outerHTML"));


            KeyValuePair<string, string> matchingPair = FindKnownXPathMatchingPair(fieldDoc, out HtmlNode? node, by);
            matchingPair.NullGuard();

            var autoGeneratedLocator = GetAutoGeneratedLocator(by, matchingPair);

            switch (matchingPair.Value)
            {
                case "dropdown":
                    throw new NotImplementedException($"OpenFieldValue: Method for field type {matchingPair.Value} has not been implemented");
                    break;
                case "textfield_autocomplete":
                    throw new NotImplementedException($"OpenFieldValue: Method for field type {matchingPair.Value} has not been implemented");
                    break;
                case "mat-autocomplete":
                    throw new NotImplementedException($"OpenFieldValue: Method for field type {matchingPair.Value} has not been implemented");
                    break;
                case "lookup_with_selection":
                    Click(autoGeneratedLocator);
                    break;
                case "lookup":
                    throw new NotImplementedException($"OpenFieldValue: Method for field type {matchingPair.Value} has not been implemented");
                    break;
                case "lookup_with_table":
                    throw new NotImplementedException($"OpenFieldValue: Method for field type {matchingPair.Value} has not been implemented");
                    break;
                case "mat-select":
                    throw new NotImplementedException($"OpenFieldValue: Method for field type {matchingPair.Value} has not been implemented");
                    break;
                default: throw new NotImplementedException($"OpenFieldValue: Method for field type {matchingPair.Value} has not been implemented");
            }
            
        }

        public string GetSelectedDropdownValue(By selectLocator)
        {
            var element = FindElementWaitUntilPresent(selectLocator);
            return new SelectElement(element).SelectedOption.Text;
            // var optionXPath = By.XPath(selectLocator.Locator.Criteria + $"//option[@selected]", selectLocator.IFrameLocator);

            // return FindElementWaitUntilPresent(optionXPath).Text;

        }

        public static bool parseStrIntoBool(string input)
        {
            input = input.ToLower();
            switch (input)
            {
                case "1":
                case "yes":
                case "true":
                case "check":
                    return true;
                case "0":
                case "no":
                case "false":
                case "uncheck":
                    return false;
                default:
                    throw new NotImplementedException("Invalid input value. Expected values for true: 1, yes, true, check. Expected values for false: 0, no, false, uncheck.");
            }
        }
        public static KeyValuePair<string, string> FindKnownXPathMatchingPair(HtmlDocument fieldDoc, out HtmlNode? node, By by)
        {
            KeyValuePair<string, string>? matchingPair = null;
            node = null;
            foreach (var pair in KnownXPaths)
            {
                var xpath = pair.Key;
                var type = pair.Value;
                var tempNode = fieldDoc.DocumentNode.SelectSingleNode(xpath);
                if (tempNode != null)
                {
                    node = tempNode;
                    matchingPair = pair;
                    break;
                }

            }

            if (node == null)
            {
                Log.Error( $"Couldn't find a a match on any of the following xpaths: {string.Join("\n", KnownXPaths.Select(it => $"{it.Value} => {it.Key}"))}");
                throw new Exception($"error finding known xpath match for field located by {by.Locator}");

            }
            return matchingPair ?? throw new ArgumentNullException();
        }
        private By GetAutoGeneratedLocator(By by, KeyValuePair<string, string> matchingPair)
        {
            var autoGeneratedLocator = By.XPath($"({by.Locator.Criteria}) {matchingPair.Key}", by.IFrameLocator );
            if (!this.ElementExists(autoGeneratedLocator))
            {
                autoGeneratedLocator = By.XPath($"({by.Locator.Criteria})", by.IFrameLocator);
            }
            return autoGeneratedLocator;
        }
        private void performLookupValueSet(By autoGeneratedLocator, string value)
        {
            Thread.Sleep(500);
            this.setText(autoGeneratedLocator, value);
            var searchButton = By.XPath(autoGeneratedLocator.Locator.Criteria + "/following-sibling::button", autoGeneratedLocator.IFrameLocator);
            this.FindElementWaitUntilClickable(searchButton).Click();
            var itemLocator = By.XPath("//ul[@aria-label='Lookup results'] //li[@aria-label]", autoGeneratedLocator.IFrameLocator);
            this.FindElementWaitUntilClickable(itemLocator).Click();
        }

        public bool WaitForTransaction(int? wait_Seconds = null)
        {
            bool result = false;
            WebDriverWait webDriverWait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            webDriverWait.IgnoreExceptionTypes(typeof(TimeoutException), typeof(NullReferenceException));
            try
            {
                if(JSExecutor.execute("return window.UCWorkBlockTracker")!=null)
                {
                    result = webDriverWait.Until((IWebDriver d) => (bool)JSExecutor.execute("return window.UCWorkBlockTracker.isAppIdle()"));
                }
            }
            catch (Exception) { }
            return result;

        }

        public void SelectDropdownValue(By selectLocator, string optionText)
        {
            var retry = Policy.Handle<Exception>()
            .WaitAndRetry(new[]
                    {
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5),
                    }
                );

            var select = new SelectElement(FindElementWaitUntilPresent(selectLocator));
            retry.Execute(()=>{

                select.SelectByText(optionText);
                if(select.SelectedOption.Text != optionText)
                {
                    Thread.Sleep(2000);
                    select.SelectByText(optionText);
                }
            });
           
            
        }
        public List<string> GetDropdownOptionsText(By selectLocator) {
            var element = this.FindElementWaitUntilPresent(selectLocator);
            var select = new SelectElement(element);
            return select.Options.Select(it => it.Text).ToList();

        }

        public bool ElementExists(By locator)
        {
            this.switchToIFrame(locator);
            this.waitForPageLoad(locator.IFrameLocator);
            this.WaitForTransaction();
            var elements = this.WebDriver.FindElements(locator.Locator);
            if (elements.Any())
                return true;
            return false;
        }
        public bool ElementExists(By locator, out IWebElement? element)
        {
            this.switchToIFrame(locator);
            this.waitForPageLoad(locator.IFrameLocator);
            this.WaitForTransaction();

            var elements = this.WebDriver.FindElements(locator.Locator);
            if (elements.Any())
            {
                element = elements.First();
                return true;
            }
            element = null;
            return false;
        }
        public bool TryClick(By locator, double waitSeconds=0)
        {
            this.switchToIFrame(locator);
            this.waitForPageLoad(locator.IFrameLocator);
            this.WaitForTransaction();
            var retries = Enumerable.Range(0, (int)(waitSeconds / 0.333))
                                   .Select(i => TimeSpan.FromSeconds(0.333 * i));
            var retry = Policy.HandleResult<bool>(false)
            .WaitAndRetry(retries);

            retry.Execute(()=>{
                if (this.ElementExists(locator, out IWebElement? element) && element.Displayed && element.Enabled)
                {
                    element.NullGuard();
                    element.Click();
                    return true;
                }
                return false;
            });
           
            return false;        
        }
        public void SendKeys(string key)
        {
            Actions action = new Actions(WebDriver);
            action.SendKeys(Keys.Enter).Build().Perform();
        }

        public void UploadFile(By dropZone, string filePath)
        {
            var element = this.FindElementWaitUntilPresent(dropZone);
            _DropFile(filePath, element, 0, 0);
        }
        private static void _DropFile(string filePath, IWebElement target, int offsetX, int offsetY)
        {
            if (!File.Exists(filePath))
                throw new WebDriverException("File not found: " + filePath);

            var driver = ((OpenQA.Selenium.WebElement)target).WrappedDriver;
            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, 30));

            String JS_DROP_FILE =
                "var target = arguments[0]," +
                "    offsetX = arguments[1]," +
                "    offsetY = arguments[2]," +
                "    document = target.ownerDocument || document," +
                "    window = document.defaultView || window;" +
                "" +
                "var input = document.createElement('INPUT');" +
                "input.type = 'file';" +
                "input.style.display = 'none';" +
                "input.onchange = function () {" +
                "  var rect = target.getBoundingClientRect()," +
                "      x = rect.left + (offsetX || (rect.width >> 1))," +
                "      y = rect.top + (offsetY || (rect.height >> 1))," +
                "      dataTransfer = { files: this.files };" +
                "" +
                "  ['dragenter', 'dragover', 'drop'].forEach(function (name) {" +
                "    var evt = document.createEvent('MouseEvent');" +
                "    evt.initMouseEvent(name, !0, !0, window, 0, 0, 0, x, y, !1, !1, !1, !1, 0, null);" +
                "    evt.dataTransfer = dataTransfer;" +
                "    target.dispatchEvent(evt);" +
                "  });" +
                "" +
                "  setTimeout(function () { document.body.removeChild(input); }, 25);" +
                "};" +
                "document.body.appendChild(input);" +
                "return input;";

            IWebElement input = (IWebElement)jse.ExecuteScript(JS_DROP_FILE, target, offsetX, offsetY);

            input.SendKeys(filePath);
        }

        public IAlert GetBrowserAlert(int? wait_Seconds=null)
        {
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            var alert = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());
            
            return alert;
        }



        private int CurrentWindowHandleIndex => this.WebDriver.WindowHandles.IndexOf(this.CurrentWindowHandle);

        public List<string> WindowHandles => this.WebDriver.WindowHandles.ToList();

        public string CurrentWindowHandle => this.WebDriver.CurrentWindowHandle;
        private void OpenNew(WindowType type)
        {
            if (type == WindowType.Tab)
            {
                ((IJavaScriptExecutor)WebDriver).ExecuteScript($"window.open('{Main.Configuration.GetVariable("HOST")}','_blank');");
                this.WebDriver.SwitchTo().Window(this.WebDriver.WindowHandles.Last());
            }
            else
            {
                this.WebDriver.SwitchTo().NewWindow(type);
                this.WebDriver.Navigate().GoToUrl(Main.Configuration.GetVariable("HOST"));
            }

        }
        public void OpenNewWindow() => OpenNew(WindowType.Window);
        public void OpenNewTab() => OpenNew(WindowType.Tab);
        public void SwitchToHandle(int index)
        {
            this.WebDriver.SwitchTo().Window(this.WebDriver.WindowHandles[index]);
        }
        public void SwitchToHandle(string handleId)
        {
            this.WebDriver.SwitchTo().Window(handleId);
        }
        public void SwitchContext(int index = -1, bool close = false)
        {
            string target;
            int windowHandlesCount = this.WebDriver.WindowHandles.Count;
            if (index == -1)
            {
                if (windowHandlesCount == 1)
                    throw new InvalidOperationException("Attempted to switch context with only a single handle invoked (run OpenNewWindow() to invoke new handles)");
                else
                {
                    var targetIndex = CurrentWindowHandleIndex == windowHandlesCount - 2 ? windowHandlesCount - 1 : windowHandlesCount - 2;
                    target = this.WebDriver.WindowHandles[targetIndex];
                }
            }
            else
            {
                target = this.WebDriver.WindowHandles[index];
            }
            if (close)
            {
                this.WebDriver.Close();
            }
            this.WebDriver.SwitchTo().Window(target);
        }

        public string Title=> this.WebDriver.Title;
    }
}
