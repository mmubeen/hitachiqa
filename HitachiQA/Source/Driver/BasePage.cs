using BoDi;
using HitachiQA.Driver;
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
        public BasePage(ObjectContainer ObjectContainer)
        {
            this.ObjectContainer = ObjectContainer;
            this.UserActions = ObjectContainer.Resolve<UserActions>();
            this.ScreenShot = ObjectContainer.Resolve<ScreenShot>();
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

        protected OpenQA.Selenium.By? _iFrame;
        public OpenQA.Selenium.By? IFrame
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(IFrameId))
                    this._iFrame = OpenQA.Selenium.By.XPath($"//iframe[@id='{IFrameId}']");
                else if (!string.IsNullOrWhiteSpace(IFrameTitle))
                    this._iFrame = OpenQA.Selenium.By.XPath($"//iframe[@title='{IFrameTitle}']");

                return _iFrame;
            }
            set { _iFrame = value; }
        }
        public string IFrameTitle;
        public string IFrameId;

        public List<string> KnownFieldXPaths = new List<string>()
        {
            "//label[text()='{input}']/..",
            "//button[text()='{input}']"
        };
        public Element GetField(string displayText) => Element(@$"({string.Join(" | ", KnownFieldXPaths.Select(it=> it.Replace("{input}", displayText)))})");
    }
}
