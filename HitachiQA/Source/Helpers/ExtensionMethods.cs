using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;


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

        public static T ToObject<T>(this object obj)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)JToken.FromObject(obj).ToString();

            }
            return JToken.FromObject(obj).ToObject<T>() ?? throw new NullReferenceException();
        }
    }
}
