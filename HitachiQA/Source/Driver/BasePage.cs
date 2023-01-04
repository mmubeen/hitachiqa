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
        public BasePage(ObjectContainer ObjectContainer)
        {
            this.UserActions = ObjectContainer.Resolve<UserActions>();
            this.ScreenShot = ObjectContainer.Resolve<ScreenShot>();
        }

        public Element Element(string xpath)
        {
            return Element(By.XPath(xpath));
        }
        public Element Element(By locator)
        {
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
    }
}
