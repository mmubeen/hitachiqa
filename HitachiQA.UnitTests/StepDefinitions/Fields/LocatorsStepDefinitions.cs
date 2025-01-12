using HitachiQA.Helpers;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace HitachiQA.UnitTests.StepDefinitions.Fields
{
    [Binding]
    public class LocatorsStepDefinitions
    {
        private IConfiguration Config;
        private SharedData SharedData { get; }

        public LocatorsStepDefinitions(IConfiguration Config, SharedData SD)
        {
            this.Config = Config;
            SharedData = SD;

        }

        [Given(@"system loads known htmls for field type '([^']*)'")]
        public void GivenSystemLoadsKnownHtmlsForFieldType(string fieldTypeName)
        {
            SharedData.SetValue("knownField", "typeName", fieldTypeName);
            string filePath = $"./Data/Fields/RawHTML/{fieldTypeName}.json";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} does not exist.");
            }

            string json = File.ReadAllText(filePath);
            var knownHTMLs = JArray.Parse(json).Select(it => it.ToString()).ToList();
            SharedData.SetValue("knownField", "knownHTMLs", knownHTMLs);


        }

        [When(@"Known xpath is queried against the known html")]
        public void WhenKnownXpathIsQueriedAgainstTheKnownHtml()
        {
            var expectedFieldType = SharedData.GetValue("knownField", "typeName");

            var knownHTMLs = SharedData.GetValue<List<string>>("knownField", "knownHTMLs");

            var matchingPairs = new List<KeyValuePair<string, string>>();

            foreach (var knownHTML in knownHTMLs)
            {
                var fieldDoc = new HtmlDocument();
                fieldDoc.LoadHtml(knownHTML);
                KeyValuePair<string, string> matchingPair = FieldAutoDetector.FindKnownXPathMatchingPair(fieldDoc, out _, "unit test placeholder");

                matchingPairs.Add(matchingPair);
            }
            SharedData.SetValue("knownField", "matchingPairs", matchingPairs);

        }

        [Then(@"a field with the previously loaded type should return")]
        public void ThenAFieldWithThePreviouslyLoadedTypeShouldReturn()
        {
            var expectedFieldType = SharedData.GetValue("knownField", "typeName");

            var matchingPairs = SharedData.GetValue<List<KeyValuePair<string, string>>>("knownField", "matchingPairs");

            foreach (var matchingPair in matchingPairs)
            {
                matchingPair.Value.Should().Be(expectedFieldType, $"expected a match on: {expectedFieldType} but instead it matched on {matchingPair.Value}");
                Log.Info($"Test Passed for: {matchingPair.Value}=>{matchingPair.Key}");
            }

        }
    }
}
