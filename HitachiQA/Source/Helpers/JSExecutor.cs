using OpenQA.Selenium;
using System;
using System.Threading;

namespace HitachiQA.Helpers
{
    public class JSExecutor
    {
        IWebDriver Driver;
        public JSExecutor(IWebDriver driver)
        {
            this.Driver = driver;
        }
        public object execute(String script, params object[] args)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            return js.ExecuteScript(script,args );
        }

        // Highligth Element
        public void highlight(IWebElement element, int borderwidth = 3)
        {
            if (borderwidth == 0)
            {
                Thread.Sleep(500);
            }
            execute(script: "arguments[0].style.border='" + borderwidth + "px solid red'", element);
        }
    }
}
