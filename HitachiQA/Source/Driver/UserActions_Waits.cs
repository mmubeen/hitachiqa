using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace HitachiQA.Driver
{
    public partial class UserActions
    {
        public int ProcessWaitParam(int? wait) => (int)(wait == null ? _defaultWaitInSecconds : wait);


        public void waitForPageLoad(By iframe = null)
        {
            if (!string.IsNullOrWhiteSpace(_loadingScreenXPath))
            {
                var locator = new[] { By.XPath(_loadingScreenXPath, iframe) };
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

        public void WaitForNetworkIdle(int timeoutSeconds = 120, int idleTimeMilis = 2000)
        {
            var retry = Policy
                .HandleResult<bool>(false)
                .WaitAndRetry(timeoutSeconds * 5, _ => TimeSpan.FromMilliseconds(200));
            retry.Execute(() =>
            {
                bool isJSComplete = (bool)((IJavaScriptExecutor)WebDriver).ExecuteScript("return document.readyState == 'complete'");

                // If you want to check for network idle, that's a bit more complex and browser-specific.
                // Here's an example of how you might do it in browsers that support the performance API:
                bool isNetworkIdle = (bool)((IJavaScriptExecutor)WebDriver)
                    .ExecuteScript(@"return performance.getEntriesByType('resource').map(x => x.startTime + x.duration).every(x => x < performance.now() - arguments[0])",
                        idleTimeMilis);

                return isJSComplete && isNetworkIdle;
            });
        }

        #region ElementPresent
        public IWebElement FindElementWaitUntilPresent(By by, int? wait_Seconds = null, bool optional = false) =>
            FindElementWaitUntilPresent(new[] { by }, wait_Seconds, optional);

        /// <summary>
        /// Find Element - Wait until element is present (different from vissible)
        /// </summary>
        public IWebElement FindElementWaitUntilPresent(By[] by, int? wait_Seconds = null, bool optional = false)
        {
            switchToIFrame(by);
            return WaitForElementAndGetWhenPresent(by.Select(b => b.Locator), out var _, wait_Seconds, optional);

        }

        /// <summary>
        /// Find Element - Wait until element is present (different from vissible)
        /// </summary>
        public IWebElement FindElementWaitUntilPresent(By[] by, out By finalBy, int? wait_Seconds = null, bool optional = false)
        {
            switchToIFrame(by);
            var result = WaitForElementAndGetWhenPresent(by.Select(b => b.Locator), out var resultBy, wait_Seconds, optional);
            finalBy = by.First(l => l.ToString() == resultBy.ToString());
            return result;

        }

        #endregion ElementPresent

        #region ElementVissible
        public IWebElement FindElementWaitUntilVisible(By[] by, int? wait_Seconds = null)
        {
            switchToIFrame(by);
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            var locator = WaitForElementAndFindMatchingCandidate(by);

            return _defaultRetry.Execute(() => {
                var target = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
                ScrollIntoView(target);
                Thread.Sleep(200);
                if (_highLightOn)
                    highlight(target);
                return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
            });
        }
        
        public List<IWebElement> FindElementsWaitUntilVisible(By by, int? wait_Seconds = null) => FindElementsWaitUntilVisible(new[] { by }, wait_Seconds);
       
        public List<IWebElement> FindElementsWaitUntilVisible(By[] by, int? wait_Seconds = null)
        {
            var locator = WaitForElementAndFindMatchingCandidate(by);
            return this.WebDriver.FindElements(locator).ToList();
        }
        #endregion ElementVissible

        #region ElementsPresent
        public List<IWebElement> FindElementsWaitUntilPresent(By by, int? wait_Seconds = null) => FindElementsWaitUntilPresent(new[] { by }, wait_Seconds);

        public List<IWebElement> FindElementsWaitUntilPresent(By[] by, int? wait_Seconds = null)
        {
            var locator = WaitForElementAndFindMatchingCandidate(by);
            return this.WebDriver.FindElements(locator).ToList();
        }
        #endregion ElementsPresent

        #region ElementClickable
        public IWebElement FindElementWaitUntilClickable(By by, int? wait_Seconds = null) => FindElementWaitUntilClickable(new[] { by }, wait_Seconds);

        public IWebElement FindElementWaitUntilClickable(By[] by, int? wait_Seconds = null)
        {
            switchToIFrame(by);
            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));
            IWebElement target = FindElementWaitUntilPresent(by, out var resultingBy);

            target = _defaultRetry.Execute(() => {
                Hover(target, true);
                if (_highLightOn)
                {
                    highlight(target);
                }
                return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(resultingBy.Locator));
            }
            );

            return target;
        }
        #endregion ElementClickable

        #region ElementDissapear
        public void WaitForElementToDisappear(By by, int? wait_Seconds = null) => WaitForElementToDisappear(new[] { by }, wait_Seconds);

        public void WaitForElementToDisappear(By[] by, int? wait_Seconds = null)
        {
            this.switchToIFrame(by);
            var locator = WaitForElementAndFindMatchingCandidate(by);

            WebDriverWait wait = new WebDriverWait(this.WebDriver, TimeSpan.FromSeconds(ProcessWaitParam(wait_Seconds)));

            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(locator));
        }
        #endregion ElementDissapear


        [DebuggerHidden]
        private IWebElement FindELelment_Retry_UntilElementPresent(OpenQA.Selenium.By locator, int? wait_Seconds = null, bool optional = false)
        {
            var retry = Policy.Handle<Exception>()
                .WaitAndRetry(ProcessWaitParam(wait_Seconds) * 3, _ => TimeSpan.FromMilliseconds(333));
            try
            {
                return retry.Execute(() =>
                {
                    var target = WebDriver.FindElement(locator);
                    ScrollIntoView(target);
                    Thread.Sleep(200);
                    if (_highLightOn)
                        highlight(target);
                    return WebDriver.FindElement(locator);
                });
            }
            catch (Exception)
            {
                if (optional)
                    return null;
                throw;
            }

        }

        private IWebElement WaitForElementAndGetWhenPresent(IEnumerable<OpenQA.Selenium.By> bys, out OpenQA.Selenium.By by, int? wait_Seconds = null, bool optional = false)
        {
            var retry =
                Policy
                .HandleResult<OpenQA.Selenium.By>(_ => _ == null)
                .WaitAndRetry(ProcessWaitParam(wait_Seconds), _ => TimeSpan.FromSeconds(1));
            waitForPageLoad();
            var inclusive = retry.Execute(() => GetFirstPresentLocator(bys, true));

            var finalBy = GetFirstPresentLocator(bys);
            by = finalBy;

            Log.Debug($"Found {by}");

            return FindELelment_Retry_UntilElementPresent(finalBy, wait_Seconds, optional);
        }

        private OpenQA.Selenium.By WaitForElementAndFindMatchingCandidate(By[] candidates) => GetFirstPresentLocator(candidates.Select(b => b.Locator));

        private OpenQA.Selenium.By GetFirstPresentLocator(IEnumerable<OpenQA.Selenium.By> candidates, bool optional = false)
        {
            //1. if only 1 candidate, return
            //2. create an exclusive xpath
            //3  check if the xpath is present in the UI (waits 0 secconds)
            //4. load the current html into a document
            //5. iterate through the candidates until one matches
            //6. return the match

            if (candidates.Count() == 1)
            {
                return candidates.First();
            }

            var inclusiveLocator = By.XPath(string.Join(" | ", candidates.Select(l => l.Criteria)));
            //the below will scroll into element
            FindElementWaitUntilPresent(inclusiveLocator, 0, true);

            var fieldDoc = new HtmlDocument();
            fieldDoc.LoadHtml(WebDriver.PageSource);
            foreach (var candidate in candidates)
            {
                var node = fieldDoc.DocumentNode.SelectSingleNode(candidate.Criteria);
                if (node != null)
                    return candidate;
            }

            if (!optional)
            {
                throw new Exception("\nNone of these xpaths were found in the UI \n" + string.Join("\n ", candidates));
            }
            return null;
        }


    }
}
