using Newtonsoft.Json.Linq;

namespace ExpeditorsBuildAutomation
{
    internal static class ExtensionMethods
    {
        public async static Task<JObject> ParseIntoJObjectAsync(this HttpResponseMessage message)
        {
            var rawStr = await message.Content.ReadAsStringAsync();
            return JObject.Parse(rawStr);
        }
    }
}
