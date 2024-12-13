using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HitachiQA.Helpers
{
    public static class ExtensionMethods
    {
        public static void NullGuard([NotNull] this object? obj, string paramName = "")
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static JObject ToJObject(this object obj) => JObject.FromObject(obj);
        public static JToken ToJToken(this object obj) => JToken.FromObject(obj);
        public static JArray ToJArray(this object obj) => JArray.FromObject(obj);

        public static T ToObject<T>(this object obj)
        {
            if (obj.GetType()==typeof(T))
            {
                return (T)obj;
            }
            if (typeof(T) == typeof(string))
            {
                return (T)(object)JToken.FromObject(obj).ToString();

            }
            return JToken.FromObject(obj).ToObject<T>() ?? throw new NullReferenceException();
        }

        public static Dictionary<string, string?>? GetDictionaryByIndex(this List<Dictionary<string, string?>> dictionaryListWithIndexKey, int index)
        {
            return dictionaryListWithIndexKey.GetDictionaryByIndex(index.ToString());

        }
        public static Dictionary<string, string?>? GetDictionaryByIndex(this List<Dictionary<string, string?>> dictionaryListWithIndexKey, string index)
        {
            return dictionaryListWithIndexKey.FirstOrDefault(dict => dict.TryGetValue("index", out string? k) && k == index);

        }

        public static void Hover(this IWebDriver driver, IWebElement element)
        {
            Actions actions = new Actions(driver);
            actions.MoveToElement(element).Perform();
        }
        public static bool IsEnable(this IWebElement element)
        {
            return element?.Enabled ?? false;
        }

        public static bool IsVisible(this IWebElement element)
        {
            return element?.Displayed ?? false;
        }
        public static bool IsClickable(this IWebElement element)
        {
            if (element.IsVisible())
            {
                return element.IsEnable();
            }

            return false;
        }
        public static object ExecuteScript(this IWebDriver driver, string script, params object[] args)
        {
            return ((IJavaScriptExecutor)driver).ExecuteScript(script, args);
        }

        public static DateTime UnixTimeStampToDateTime(this long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
