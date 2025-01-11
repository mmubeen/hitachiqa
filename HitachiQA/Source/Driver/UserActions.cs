using HitachiQA.Helpers;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Polly;
using Reqnroll.BoDi;

namespace HitachiQA.Driver
{
    public partial class UserActions
    {
        private readonly IWebDriver WebDriver;
        private readonly IConfiguration Configuration;
        private readonly JSExecutor JSExecutor;
        private readonly Policy _defaultRetry;


        /// <summary>
        ///Most applications have some sort of loading screen, please allow this variable to hold the that locator. please set this xpath in your appsettings.json file as LOADING_SCREEN_XPATH
        /// </summary>
        private readonly string _loadingScreenXPath = "";
        private readonly int _defaultWaitInSecconds = 30;
        private readonly bool _highLightOn = false;
        private readonly string _applicationType;

        public UserActions(IObjectContainer objectContainer)
        {
            WebDriver = objectContainer.Resolve<IWebDriver>();
            Configuration = objectContainer.Resolve<IConfiguration>();
            JSExecutor = objectContainer.Resolve<JSExecutor>();

            var configKeys = this.Configuration.GetChildren();
            var wait = configKeys.FirstOrDefault(it => it.Key == "DEFAULT_WAIT_SECONDS");
            var highlight = configKeys.FirstOrDefault(it => it.Key == "HIGHLIGHT_ON");
            var loadingXPath = configKeys.FirstOrDefault(it => it.Key == "LOADING_SCREEN_XPATH");
            _applicationType = Configuration.GetVariable("APPLICATION_TYPE", true);

            if (wait != null)
            {
                _defaultWaitInSecconds = int.Parse(wait.Value);
            }
            if (highlight != null)
            {
                _highLightOn = bool.Parse(highlight.Value);
            }
            if (loadingXPath != null)
            {
                _loadingScreenXPath = loadingXPath.Value;
            }
            _defaultRetry = Policy
                .Handle<ElementClickInterceptedException>()
                .Or<StaleElementReferenceException>()
                .WaitAndRetry(3, i => TimeSpan.FromSeconds(1));
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
        }

        public void Back()
        {
            this.WebDriver.Navigate().Back();
        }

        //
        // General Element Actions
        //

        public string getElementText(By[] ElementLocator, int? wait_Seconds = null)
        {
            var textField = FindElementWaitUntilPresent(ElementLocator, ProcessWaitParam(wait_Seconds));
            return textField.Text.Trim();
        }

        public bool Click(By ElementLocator, int? wait_Seconds = null, bool optional = false) => Click(new[] { ElementLocator }, wait_Seconds, optional);

        public bool Click(By[] ElementLocator, int? wait_Seconds = null, bool optional = false)
        {
            try
            {
                _defaultRetry.Execute(() =>
                {
                    WaitForTransaction();
                    waitForPageLoad(ElementLocator.First().IFrameLocator);
                    FindElementWaitUntilClickable(ElementLocator, ProcessWaitParam(wait_Seconds)).Click();
                });
            }
            catch (Exception ex)
            {
                if (optional)
                {
                    return false;
                }
                throw new Exception($"\nClick() failed Locator: {Log.stringify(ElementLocator)}\n", ex);
            }
            return true;
        }
        public bool DoubleClick(By ElementLocator, int? wait_Seconds = null, bool optional = false) =>
            DoubleClick(new[] { ElementLocator }, wait_Seconds, optional);

        public bool DoubleClick(By[] ElementLocator, int? wait_Seconds = null, bool optional = false)
        {
            Actions Action = new Actions(this.WebDriver);
            try
            {
                _defaultRetry.Execute(() =>
                {
                    var element = FindElementWaitUntilPresent(ElementLocator, ProcessWaitParam(wait_Seconds));
                    Action.MoveToElement(element).DoubleClick(element).Build().Perform();
                });
            }
            catch (Exception ex)
            {
                Functions.HandleFailure($"DoubleClick() failed Locator: {Log.stringify(ElementLocator)}", ex, optional);
                return false;
            }
            return true;
        }

        public bool GetIsDisabled(By[] elementLocator)
        {
            var element = FindElementWaitUntilPresent(elementLocator);

            return !element.Enabled;
        }
        public bool GetIsDisplayed(By[] elementLocator)
        {
            var element = FindElementWaitUntilPresent(elementLocator);

            return element.Displayed;
        }

        public string GetAttribute(By[] ElementLocator, string attributeName)
        {
            return FindElementWaitUntilClickable(ElementLocator).GetDomAttribute(attributeName);
        }

        private void switchToIFrame(By[] bys)
        {
            this.WebDriver.SwitchTo().DefaultContent();
            if (bys.Select(l => l.IFrameLocator?.ToString())?.Distinct()?.Count() > 1)
            {
                throw new InvalidOperationException("Provided locators have different IFrameLocators");
            }
            var by = bys.First();

            if (by.IFrameLocator != null)
            {
                if (by.IFrameLocator.IFrameLocator != null)
                {
                    switchToIFrame(new By[] { by.IFrameLocator });
                }
                var frameElement = this.FindELelment_Retry_UntilElementPresent(by.IFrameLocator.Locator);
                this.WebDriver.SwitchTo().Frame(frameElement);
            }

        }

        public void Hover(IWebElement target, bool optional = false)
        {

            ScrollIntoView(target);
            var action = new Actions(this.WebDriver);
            try
            {
                action.MoveToElement(target).Build().Perform();
            }
            catch (Exception)
            {
                if (!optional)
                {
                    throw;
                }
            }
        }

        //
        //  Text Fields Actions
        //
        public void setText(By TextFieldLocator, string TextToEnter, int? wait_Seconds = null)
            => setText(new[] { TextFieldLocator }, TextToEnter, ProcessWaitParam(wait_Seconds));

        public void setText(By[] TextFieldLocator, string TextToEnter, int? wait_Seconds = null)
        {
            Actions Action = new Actions(this.WebDriver);
            var textField = FindElementWaitUntilClickable(TextFieldLocator, ProcessWaitParam(wait_Seconds));
            Action.MoveToElement(textField).Click(textField).Build().Perform();
            textField.SendKeys(Keys.Control + "a");
            textField.SendKeys(Keys.Delete);
            textField.SendKeys(TextToEnter);
        }

        public string getTextFieldText(By[] TextFieldLocator, int? wait_Seconds = null)
        {
            var textField = FindElementWaitUntilVisible(TextFieldLocator, ProcessWaitParam(wait_Seconds));
            return textField.GetDomAttribute("value");
        }

        public void clearTextField(By[] TextFieldLocator, int? wait_Seconds = null)
        {
            var textField = FindElementWaitUntilVisible(TextFieldLocator, ProcessWaitParam(wait_Seconds));
            textField.SendKeys(Keys.Control + "a");
            textField.SendKeys(Keys.Delete);
        }


        //
        // Radio Button
        //

        public bool IsRadioButtonSelected(By[] RadioButtonLocator)
        {
            var radioButton = FindElementWaitUntilPresent(RadioButtonLocator);

            return radioButton.Selected;
        }

        //
        // Checkbox
        //

        public bool GetCheckboxState(By CheckBoxInputLocator) => GetCheckboxState(new[] { CheckBoxInputLocator });

        public bool GetCheckboxState(By[] CheckBoxInputLocator)
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

        public IEnumerable<Dictionary<string, string>> parseUITable(By datatable)
        {
            var tableElement = FindElementWaitUntilPresent(datatable);
            //Mat UI bootstrap table
            if (this.ElementExists(By.XPath(datatable.Locator.Criteria + "//datatable-header-cell", datatable.IFrameLocator)))
            {

                var datatableXpath = datatable.Locator.Criteria;
                List<string> columnNames = this.WebDriver.FindElements(By.XPath(datatableXpath + "//datatable-header-cell//span[contains(@class,'datatable-header-cell-label')]", datatable.IFrameLocator).Locator).Select(element => element.Text).ToList<string>();

                int rowCount = this.WebDriver.FindElements(By.XPath(datatableXpath + "//datatable-body-row", datatable.IFrameLocator).Locator).Count;
                for (int rowIndex = 1; rowIndex <= rowCount; rowIndex++)
                {
                    var rowDict = new Dictionary<string, string>();

                    for (int i = 0; i < columnNames.Count(); i++)
                    {
                        // string cellText = string.Join("", cells[i].FindElements(By.XPath("/descendant::*"))
                        string cellText = string.Join("", this.WebDriver
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


                if (this.FindElementsWaitUntilPresent(By.XPath($"({datatable.Locator.Criteria} {rowsXPath})[1] {dataXPath}", datatable.IFrameLocator)).Count == 1 && headers.Count != 1)
                {
                    yield return headers.ToDictionary(it => !string.IsNullOrEmpty(it.Value) ? it.Value : it.Key.ToString(), it => "");
                }
                else
                {
                    var tableDoc = new HtmlDocument();
                    tableDoc.LoadHtml(tableElement.GetDomAttribute("innerHTML"));
                    var rows = tableDoc.DocumentNode.SelectNodes(rowsXPath);
                    var rowIndex = 0;
                    foreach (var rowNode in rows)
                    {
                        var rowDict = new Dictionary<string, string>();
                        rowDict.Add("index", rowIndex.ToString());
                        var rowHTML = rowNode.InnerHtml;
                        var rowDoc = new HtmlDocument();
                        rowDoc.LoadHtml(rowHTML);
                        var row = rowDoc.DocumentNode;
                        var cells = row.SelectNodes(dataXPath);
                        var cellIndex = 0;
                        foreach (var cell in cells)
                        {
                            var header = headers[cellIndex] ?? throw new NullReferenceException();
                            if (string.IsNullOrWhiteSpace(header))
                            {
                                header = cellIndex.ToString();
                            }
                            var children = cell.ChildNodes.Select(it => it.InnerText.Trim()).ToList();
                            children.Add(cell.InnerText.Trim());
                            var cellText = string.Join("", children.Distinct()) ?? "";
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
        public Dictionary<int, string> GetUITableHeaders(By table)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();

            var tableElement = FindElementWaitUntilPresent(table);

            var tableDoc = new HtmlDocument();
            tableDoc.LoadHtml(tableElement.GetDomAttribute("innerHTML"));
            var headersXPath = "//th[text()]/..//th";
            var headers = tableDoc.DocumentNode.SelectNodes(headersXPath);
            if (headers == null || headers.Count == 0)
            {
                headers = tableDoc.DocumentNode.SelectNodes("//th");
            }
            var index = 0;
            foreach (var header in headers)
            {
                var children = header.ChildNodes.Select(it => it.InnerText.Trim()).ToList();
                children.Add(header.InnerText.Trim());

                var cellText = string.Join("", children.Distinct()) ?? "";
                cellText = System.Web.HttpUtility.HtmlDecode(cellText);

                result.Add(index, cellText);
                index++;
            }
            return result;

        }

        public string GetSelectedDropdownValue(By[] selectLocator)
        {
            var element = FindElementWaitUntilPresent(selectLocator);
            return new SelectElement(element).SelectedOption.Text;
        }



        public void SelectDropdownValue(By[] selectLocator, string optionText)
        {
            var retry = Policy.Handle<Exception>()
            .WaitAndRetry(new[]
                    {
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5),
                    }
                );

            retry.Execute(() =>
            {
                var select = new SelectElement(FindElementWaitUntilPresent(selectLocator));

                select.SelectByText(optionText);
                if (select.SelectedOption.Text != optionText)
                {
                    Thread.Sleep(2000);
                    select.SelectByText(optionText);
                }
            });


        }
        public List<string> GetDropdownOptionsText(By[] selectLocator)
        {
            var element = this.FindElementWaitUntilPresent(selectLocator);
            var select = new SelectElement(element);
            return select.Options.Select(it => it.Text).ToList();

        }

        public bool ElementExists(By locator) => ElementExists(new[] { locator });
        public bool ElementExists(By[] locators)
        {
            switchToIFrame(locators);
            waitForPageLoad(locators.First().IFrameLocator);
            WaitForTransaction();
            var elements = locators.SelectMany(locator => WebDriver.FindElements(locator.Locator));
            if (elements.Any())
                return true;
            return false;
        }

        public bool ElementExists(By[] locators, out IWebElement element)
        {
            switchToIFrame(locators);
            waitForPageLoad(locators.First().IFrameLocator);
            WaitForTransaction();

            var elements = locators.SelectMany(locator => WebDriver.FindElements(locator.Locator));
            if (elements.Any())
            {
                element = elements.First();
                return true;
            }
            element = null;
            return false;
        }
        public bool TryClick(By[] locator, double waitSeconds = 0)
        {
            switchToIFrame(locator);
            waitForPageLoad(locator.First().IFrameLocator);
            WaitForTransaction();
            var retries = Enumerable.Range(0, (int)(waitSeconds / 0.333))
                                   .Select(i => TimeSpan.FromSeconds(0.333 * i));
            var retry = Policy.HandleResult<bool>(false)
            .WaitAndRetry((int)waitSeconds, _ => TimeSpan.FromSeconds(1));

            var interceptRetry = Policy.Handle<ElementClickInterceptedException>()
            .WaitAndRetry(2, _ => TimeSpan.FromSeconds(1));

            return retry.Execute(() =>
            {
                if (this.ElementExists(locator, out IWebElement element) && element.Displayed && element.Enabled)
                {
                    element.NullGuard();
                    interceptRetry.Execute(() => element.Click());
                    return true;
                }
                return false;
            });

        }
        public void SendKeys(string key)
        {
            Actions action = new Actions(WebDriver);
            action.SendKeys(Keys.Enter).Build().Perform();
        }

        public void UploadFile(By[] dropZone, string filePath)
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

            string JS_DROP_FILE =
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

        public IAlert GetBrowserAlert(int? wait_Seconds = null)
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

        public string Title => this.WebDriver.Title;
    }
}
