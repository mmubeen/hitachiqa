using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using seleniumBy = OpenQA.Selenium.By;
namespace HitachiQA.Driver
{
    public class By
    {
        public seleniumBy? IFrameLocator;
        public seleniumBy Locator;

        public By(seleniumBy locator)
        {
            this.Locator = locator;
        }
        public By(seleniumBy locator, seleniumBy? iframe) : this(locator)
        {
            this.IFrameLocator = iframe;
        }
        public static By XPath(string xpathToFind)
        {
            return new By(seleniumBy.XPath(xpathToFind));
        }
        public static By XPath(string xpathToFind, seleniumBy? iFrameLocator)
        {
            return new By(seleniumBy.XPath(xpathToFind), iFrameLocator);
        }
        public static By XPath(seleniumBy locatorToFind, seleniumBy? iFrameLocator)
        {
            return new By(locatorToFind, iFrameLocator);
        }
        public static By Id(string idToFind, seleniumBy? iFrameLocator)
        {
            return new By(seleniumBy.XPath(idToFind), iFrameLocator);
        }
        public static By Id(string idToFind)
        {
            return new By(seleniumBy.XPath(idToFind));
        }

        public override string ToString()
        {
            return this.Locator.ToString();
        }
    }
}
