using BoDi;
using HitachiQA.Driver;
using HitachiQA.Hooks;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Driver
{
    public class BasePage
    {
        public readonly UserActions UserActions;
        public readonly ScreenShot ScreenShot;
        protected readonly ObjectContainer ObjectContainer;
        private DriverManager DriverManager;
        public BasePage(ObjectContainer ObjectContainer)
        {
            this.ObjectContainer = ObjectContainer;
            this.UserActions = ObjectContainer.Resolve<UserActions>();
            this.ScreenShot = ObjectContainer.Resolve<ScreenShot>();
            this.DriverManager = ObjectContainer.Resolve<DriverManager>();

        }

        public Element Element(string xpath)
        {
            return Element(By.XPath(xpath));
        }
        public Element Element(By locator)
        {
            if(this.IFrame!=null && locator.IFrameLocator==null)
            {
                locator.IFrameLocator = this.IFrame;
            }
            return new Element(locator, UserActions);
        }

        public void ScrollToBottom()
        {
            UserActions.ScrollToBottom();
        }

       

        public void ScrollToTop()
        {
            UserActions.ScrollToTop();
        }

        public string GetCurrentURL()
        {
            return UserActions.GetCurrentURL();
        }

        public string GetCurrentURLPath()
        {
            return new Uri(UserActions.GetCurrentURL()).PathAndQuery;
        }

        public void refreshPage()
        {
            UserActions.Refresh();
        }

        public void Navigate(string PATH_OR_URL)
        {
            UserActions.Navigate(PATH_OR_URL);
        }
        public void PressEnter()
        {
            UserActions.SendKeys(Keys.Enter);
        }
        /// <summary>
        /// example: Keys.Enter
        /// </summary>
        /// <param name="key"></param>
        public void SendKeys(string key)
        {
            UserActions.SendKeys(Keys.Enter);
        }
        public void WaitForPage_And_Transactions()
        {
            this.UserActions.WaitForTransaction();
            this.UserActions.waitForPageLoad(_iFrame);
        }

        protected By? _iFrame;
        public By? IFrame
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(IFrameId))
                    this._iFrame = By.XPath($"//iframe[@id='{IFrameId}']");
                else if (!string.IsNullOrWhiteSpace(IFrameTitle))
                    this._iFrame = By.XPath($"//iframe[@title='{IFrameTitle}']");
                else if (!string.IsNullOrWhiteSpace(IFrameName))
                    this._iFrame = By.XPath($"//iframe[@name='{IFrameName}']");

                return _iFrame;
            }
            set { _iFrame = value; }
        }
        public string IFrameTitle;
        public string IFrameId;
        public string IFrameName;

        public static List<string> KnownFieldXPaths = new List<string>()
        {
            "//label[text()='{input}']/..",
            "//button[normalize-space(text())='{input}']",
            "//*[@data-id='{input}']",
            "//button[.//*[normalize-space(text())='{input}']]",
            "//a[.//*[normalize-space(text())='{input}']]",
            "//a[normalize-space(text())='{input}']",
            "//a[@title='{input}']",
            "//button[@data-id='{input}']",
            "//*[@aria-label='{input}']",
            "//li[@title='{input}']",
            "//td[@data-hslcolumnname='{input}']",
            "//button[@id='{input}']",
            "//button[@title='{input}']",
            "//label[normalize-space(text())='{input}']/following-sibling::input"
        };
        public static List<string> KnownParents = new List<string>()
        {
            "//*[@id='jd-page-{input}']"
        };
        public Element GetField(string displayText_or_logicalName) => Element(@$"({string.Join(" | ", KnownFieldXPaths.Distinct().Select(it=> it.Replace("{input}", displayText_or_logicalName)))}) /self::*[not(contains(@style,'display: none'))]");

        public Element GetField(string parentDisplayText_or_logicalName, string displayText_or_logicalName)
        {
            List<string> finalXPaths = new List<string>();
            var xpaths = KnownFieldXPaths.Select(it => it.Replace("{input}", displayText_or_logicalName));

            foreach(var childXPath in xpaths)
            {
                var possibleParents = KnownParents.Select(it => it.Replace("{input}", parentDisplayText_or_logicalName));
                finalXPaths.AddRange(possibleParents.Select(parentXPath => parentXPath + childXPath));
            }

            return Element(string.Join(" | ", finalXPaths));
        }
        public Element GetField(By parent, string fieldDisplayText_or_logicalName)
        {
           
            List<string> finalXPaths = new List<string>();
            var xpaths = KnownFieldXPaths.Select(it => it.Replace("{input}", fieldDisplayText_or_logicalName));

            foreach(var childXPath in xpaths)
            {
                finalXPaths.Add(parent.Locator.Criteria + childXPath);
            }

            return Element(string.Join(" | ", finalXPaths));
        }

        public void AcceptBrowserAlert()
        {
            GetBrowserAlert().Accept();
        }
        public IAlert GetBrowserAlert()
        {
            return UserActions.GetBrowserAlert();
        }

        public List<string> WindowHandles => this.UserActions.WindowHandles;
        public string CurrentWindowHandle => this.UserActions.CurrentWindowHandle;
        public void OpenNewWindow() => UserActions.OpenNewWindow();
        public void OpenNewTab() => UserActions.OpenNewTab();
        public void SwitchToHandle(int index) => UserActions.SwitchToHandle(index);
        public void SwitchToHandle(string handleId) => UserActions.SwitchToHandle(handleId);
        public void SwitchContext(int index = -1, bool close = false) => UserActions.SwitchContext(index, close);
        public void SwitchContextAndCloseCurrentHandle(int index = -1) => this.SwitchContext(index, true);
        public string Title => UserActions.Title;
    }
}
