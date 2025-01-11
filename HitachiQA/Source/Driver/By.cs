using seleniumBy = OpenQA.Selenium.By;
namespace HitachiQA.Driver
{
    public class By
    {
        public By IFrameLocator;
        public seleniumBy Locator;

        public By(seleniumBy locator)
        {
            this.Locator = locator;
        }
        public By(seleniumBy locator, By iframe) : this(locator)
        {
            this.IFrameLocator = iframe;
        }
        public static By XPath(string xpathToFind)
        {
            return new By(seleniumBy.XPath(xpathToFind));
        }
        public static By XPath(string xpathToFind, By iFrameLocator)
        {
            return new By(seleniumBy.XPath(xpathToFind), iFrameLocator);
        }
        public static By XPath(seleniumBy locatorToFind, By iFrameLocator)
        {
            return new By(locatorToFind, iFrameLocator);
        }
        public static By Id(string idToFind, By iFrameLocator)
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
