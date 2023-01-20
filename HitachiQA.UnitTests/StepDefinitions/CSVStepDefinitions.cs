using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HitachiQA.Helpers;
using HitachiQA.UnitTests.Data;

namespace HitachiQA.UnitTests.StepDefinitions
{
    [Binding]
    public class CSVStepDefinitions
    {
        private List<Dictionary<String, String>>? csvResults;

        public List<string> KVPairs { get; } = new();

        [Given(@"User parses the input CSV file from Data folder")]
        public void GivenUserParsesTheInputCSVFileFromDataFolder()
        {
            string filePath = "./Data/CSV/addresses.csv";
            csvResults = Functions.parseCSV(filePath);
        }

        [When(@"User gets the data present in the CSV")]
        public void WhenUserGetsTheDataPresentInTheCSV()
        {
            var collection = csvResults;

            if (collection != null)
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    Dictionary<String, String> dict = collection.ElementAt(i);
                    foreach (KeyValuePair<String, String> kvp in dict)
                    {
                        Console.WriteLine("Key: {0} Value: {1}", kvp.Key, kvp.Value); //Key Value Pairs for Parsed CSV data
                        KVPairs.Add($"Key: {kvp.Key}, Value: {kvp.Value}");
                    }
                }
            }
        }

        [Then(@"User validates the data is parsed correctly from the CSV")]
        public void ThenUserValidatesTheDataIsParsedCorrectlyFromTheCSV()
        {
            KVPairs.SequenceEqual(InputData.CSVInputKVP).Should().BeTrue();
        }
    }
}
