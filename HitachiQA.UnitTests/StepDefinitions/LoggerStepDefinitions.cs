using HitachiQA.Helpers;
using System;
using TechTalk.SpecFlow;
using Newtonsoft.Json.Linq;

namespace HitachiQA.UnitTests.StepDefinitions
{
    [Binding]
    public class LoggerStepDefinitions
    {
        Dictionary<string, object> inputs= new Dictionary<string, object>() {

            {"string[]", new string[]{"apple", "strawberry", "banana" } },
            {"JArray", new JArray(){null,null,null}},
            {"Dictionary", new Dictionary<string,object> { { "id", 123 }, {"name","miguel" } } },
            {"JObject", new JObject(){ { "id", 123 }, { "name", "miguel" } } },
            {"string", "test string here 123\n123" },
            {"long", 1231231232 },
            {"decimal", 123123234234.9034m }
        };
        string[] strArr = new string[] { "apple", "strawberry", "banana" };
        JArray nullArr = new JArray() { null,null,null };

        object value;
        string result;


        [When(@"user stringifies '([^']*)'")]
        public void WhenUserStringifies(string input)
        {
            switch(input)
            {
                case "NULL":
                    result = Log.stringify(null);
                    break;
                default:
                    value = inputs[input];
                    result = Log.stringify(value);
                    break;
            }
        }

        [Then(@"the expected '([^']*)' should be returned")]
        public void ThenTheExpectedShouldBeReturned(string outcome)
        {
            switch (outcome)
            {
                case "NULL":
                    result.Should().Be("NULL");
                    break;
                case "SameAsInput":
                    result.Should().Be(value is string? (string)value : value.ToString());
                    break;
                default:
                    result.Should().Be(JToken.FromObject(value).ToString());
                    break;

            }
        }


    }
}
