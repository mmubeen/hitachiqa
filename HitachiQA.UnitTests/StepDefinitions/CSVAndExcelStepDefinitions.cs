using HitachiQA.Helpers;
using HitachiQA.UnitTests.Data.CSV_EXCEL_PARSING;

namespace HitachiQA.UnitTests.StepDefinitions
{
    [Binding]
    public class CSVAndExcelStepDefinitions
    {
        private List<Dictionary<string, string>> csvResults;
        private IEnumerable<Dictionary<string, string>> excelResult;
        private readonly HashSet<string> keys = new();
        private readonly HashSet<string> values = new();
        public List<string> KVPairs { get; } = new();

        [Given(@"User parses the input CSV file from Data folder")]
        public void GivenUserParsesTheInputCSVFileFromDataFolder()
        {
            string filePath = "./Data/CSV_EXCEL_PARSING/addresses.csv";
            csvResults = Functions.ParseCSV(filePath);
        }

        [When(@"User gets the data present in the CSV")]
        public void WhenUserGetsTheDataPresentInTheCSV()
        {
            var collection = csvResults;

            if (collection != null)
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    Dictionary<string, string> dict = collection.ElementAt(i);
                    foreach (KeyValuePair<string, string> kvp in dict)
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

        [Given(@"User parses the input Excel file from Data folder")]
        public async Task GivenUserParsesTheInputExcelFileFromDataFolder()
        {
            string filePath = "./Data/CSV_EXCEL_PARSING/CountryCapitals.xlsx";
            excelResult = await Functions.ParseExcelAsync(filePath);
        }

        [When(@"User gets the data present in the Excel sheet")]
        public void WhenUserGetsTheDataPresentInTheExcelSheet()
        {
            var collection = excelResult;
            if (collection != null)
            {
                for (int i = 0; i < collection.Count(); i++)
                {
                    Dictionary<string, string> dict = collection.ElementAt(i);
                    foreach (KeyValuePair<string, string> kvp in dict)
                    {
                        Console.WriteLine("Key: {0}, Value: {1}", kvp.Key, kvp.Value);
                        keys.Add(kvp.Key);
                        values.Add(kvp.Value);
                    }
                }
            }
        }

        [Then(@"User validates the data is parsed correctly from the Excel")]
        public void ThenUserValidatesTheDataIsParsedCorrectly()
        {
            (keys.Intersect(InputData.ExcelInputKeys).Count() == keys.Count).Should().BeTrue();

            (values.Intersect(InputData.ExcelInputValues).Count() == values.Count).Should().BeTrue();
        }
    }
}
