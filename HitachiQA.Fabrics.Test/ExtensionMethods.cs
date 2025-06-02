using Newtonsoft.Json.Linq;

namespace HitachiQA.Fabrics.Test
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
