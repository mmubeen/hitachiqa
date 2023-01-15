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
using OpenQA.Selenium.DevTools.V103.IndexedDB;

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


        public void waitForPageLoad()
        {
            if (!string.IsNullOrWhiteSpace(LOADING_SCREEN_XPATH))
            {
                By locator = By.XPath(LOADING_SCREEN_XPATH);
                //this is optional
                try
                {
                    FindElementWaitUntilVisible(locator, 1);
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

        private void switchToIFrame(By by)
        {
            if (by.IFrameLocator == null)
            {
                this.WebDriver.SwitchTo().DefaultContent();
            }
            else
            {
                var frameElement = this.FindElementWaitUntilPresent(by.IFrameLocator);
                this.WebDriver.SwitchTo().Frame(frameElement);

            }

        }
        //Find Element - Wait until element is present (different from vissible)
        public IWebElement FindElementWaitUntilPresent(By by, int? wait_Seconds = null)
        {
            this.switchToIFrame(by);
            var locator = by.Locator;
            return this.FindElementWaitUntilPresent(locator);

        }

        public IWebElement FindElementWaitUntilPresent(OpenQA.Selenium.By locator, int? wait_Seconds = null)
        {
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
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

            ScrollIntoView(target);
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

        //Find Element - Wait Until Clickable
        public IWebElement FindElementWaitUntilClickable(By by, int? wait_Seconds = null)
        {
            switchToIFrame(by);
            var locator = by.Locator;
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            IWebElement target;


            target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));
            ScrollIntoView(target);
            target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
            ScrollIntoView(target);

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

        public IEnumerable<Dictionary<String, String>> parseUITable(string datatableXpath)
        {
            FindElementWaitUntilPresent(By.XPath(datatableXpath));
            List<String> columnNames = this.WebDriver.FindElements(By.XPath(datatableXpath + "//datatable-header-cell//span[contains(@class,'datatable-header-cell-label')]").Locator).Select(element => element.Text).ToList<String>();

            int rowCount = this.WebDriver.FindElements(By.XPath(datatableXpath + "//datatable-body-row").Locator).Count;
            for (int rowIndex = 1; rowIndex <= rowCount; rowIndex++)
            {
                var rowDict = new Dictionary<String, String>();

                for (int i = 0; i < columnNames.Count(); i++)
                {
                    // String cellText = string.Join("", cells[i].FindElements(By.XPath("/descendant::*"))
                    String cellText = string.Join("", this.WebDriver
                                                      .FindElements(By.XPath($"(({datatableXpath} //datatable-body-row)[{rowIndex}] //datatable-body-cell)[{i + 1}]/descendant::*").Locator)
                                                      .Select(child => child.Text).Distinct());

                    rowDict.Add(columnNames[i], cellText.Trim());
                }
                yield return rowDict;
            }
        }

        private const string HORIZONTAL_SCROLL_BAR = "//div[@class='ag-body-horizontal-scroll'] //div[@ref='eViewport']";
        private object GRID_SCROLL_JS_EXEC(By gridLocator, string command)
        {
            var scrollBarLoc = $"{gridLocator.Locator.Criteria} {HORIZONTAL_SCROLL_BAR}";
            var element = this.FindElementWaitUntilPresent(By.XPath(scrollBarLoc));
            return JSExecutor.execute(command, element);
        }

        private object GRID_SCROLL_LEFT_COMMAND(By gridLocator) => GRID_SCROLL_JS_EXEC(gridLocator, "arguments[0].scrollBy(-400, 0);");
        private object GRID_SCROLL_ALL_RIGHT_COMMAND(By gridLocator) => GRID_SCROLL_JS_EXEC(gridLocator, "arguments[0].scrollBy(12000, 0);");
        private object GRID_QUERY_HOW_MUCH_UNTIL_RESET(By gridLocator) => GRID_SCROLL_JS_EXEC(gridLocator, "arguments[0].scrollLeft;");

       


        public Dictionary<int, string?> GetGridHeaders(By by)
        {
            this.WaitForTransaction();

            string gridCellXPath = $"//*[@role='columnheader']";

            Dictionary<int, string?> result = new Dictionary<int, string?>();


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
                    }


                    if (!result.ContainsKey(index))
                    {
                        result.Add(index, displayText);
                    }
                }

                GRID_SCROLL_LEFT_COMMAND(by);
            }
            while (Convert.ToDouble(GRID_QUERY_HOW_MUCH_UNTIL_RESET(by)) != 0);

            return result;
        }

        public List<Dictionary<string, string?>> GetGridItems(By by)
        {
            this.WaitForTransaction();
           

            var headers = this.GetGridHeaders(by);
            var results = new List<Dictionary<string, string?>>();

            var rowXPath = $"//*[@role='row' and descendant::*[@aria-colindex] ]";

            GRID_SCROLL_ALL_RIGHT_COMMAND(by);
            do
            {
                var gridElement = this.FindElementWaitUntilPresent(by);
                var gridDoc = new HtmlDocument();
                gridDoc.LoadHtml(gridElement.GetAttribute("innerHTML"));

                var rows = gridDoc.DocumentNode.SelectNodes(rowXPath);
                //remove header (1st row)
                rows.RemoveAt(0);
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
                                cellValue = cellNode.GetAttributeValue(attr, null);
                                break;
                            }

                        }

                     
                        if (rowDict.GetValueOrDefault(headerName) == null || !string.IsNullOrWhiteSpace(cellValue))
                        {
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
            var gridItems = this.GetGridItems(by);

            var matchingRow = gridItems.FirstOrDefault(row=> row.TryGetValue(columnName, out var colVal) && colVal == value);

            if(matchingRow==null)
            {
                throw new NotFoundException($"Couldn't find row matching {columnName}={value}");
            }

            var index = matchingRow["index"];

            var checkBoxLoc = By.XPath(by.Locator.Criteria + $"//*[@role='row' and @aria-rowindex={int.Parse(index)+1} and descendant::*[@aria-colindex=1] ]");
            this.DoubleClick(checkBoxLoc);

            //var EditButtonLoc = By.XPath("((//div[@id='mainContent'] //*[contains(@data-id, 'Command')])[1] | //*[@data-id='OverflowFlyout']) //button[*//text()='Edit']");

            //this.FindElementWaitUntilClickable(by).Click();
            this.WaitForTransaction();

        }

        public static Dictionary<string, string> KnownXPaths = new Dictionary<string, string> {
                { "//select[contains(@data-id, 'option-set-select')]", "dropdown" },
                { "//input[@type='text' and contains(@data-id, 'text-box-text')]", "textfield" },
                { "//input[@type='text' and contains(@data-id, 'text-input')]", "textfield" },
                { "//textarea[@type='text' and @aria-autocomplete='list'] ", "textfield_autocomplete" },
                { "//input[@type='text' and contains(@data-id, 'textInputBox_with_filter')]", "lookup" },
                { "//*[@role='link' and contains(@id, 'selected_tag')]", "lookup_with_selection" },
                { "//*[@role='switch']", "switch" },
                { "//input[@type='text' and following-sibling::*[contains(@data-dyn-bind, 'Lookup')]]", "lookup_with_table" }
            };

        public void SetFieldValue(By by, string value) {
            this.WaitForTransaction();

            var fieldElement = this.FindElementWaitUntilPresent(by);
            var fieldDoc = new HtmlDocument();
            fieldDoc.LoadHtml(fieldElement.GetAttribute("innerHTML"));


            KeyValuePair<string, string> matchingPair = FindKnownXPathMatchingPair(fieldDoc, out HtmlNode? node); 

            matchingPair.NullGuard();

            var autoGeneratedLocator = By.XPath($"{by.Locator.Criteria} {matchingPair.Key}", by.IFrameLocator );

            switch(matchingPair.Value)
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

                    if(bool.Parse(element.GetAttribute("aria-checked")) != switchVal)
                        element.Click();
                    

                    break;
                case "lookup_with_table":
                    this.setText(autoGeneratedLocator, value);
                    //index=1 is the header and index=2 is the first row
                    this.Click(By.XPath("//form[contains(@class, 'lookup-popup active-form')]//*[@role='row' and @aria-rowindex='2']"));


                    break;
                default: throw new NotImplementedException($"Method for field type {matchingPair.Value} has not been implemented");
            }
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
        public static KeyValuePair<string, string> FindKnownXPathMatchingPair(HtmlDocument fieldDoc, out HtmlNode? node)
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
                throw new NotImplementedException($"Couldn't find a a match on any of the following xpaths: {string.Join("\n", KnownXPaths.Select(it => $"{it.Value} => {it.Key}"))}");
            }
            return matchingPair ?? throw new ArgumentNullException();
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
            FindElementWaitUntilClickable(selectLocator).Click();

            var optionXPath = By.XPath(selectLocator.Locator.Criteria + $"//option[text()='{optionText}']", selectLocator.IFrameLocator);

            FindElementWaitUntilPresent(optionXPath).Click();
            
        }
    }
}
