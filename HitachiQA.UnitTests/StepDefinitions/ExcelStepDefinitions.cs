using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HitachiQA.Helpers;
using HitachiQA.UnitTests.Data;
using Microsoft.Extensions.Configuration;

namespace HitachiQA.UnitTests.StepDefinitions
{
    [Binding]
    public class ExcelStepDefinitions
    {
        private IEnumerable<Dictionary<String, String>>? excelResult;
        private readonly HashSet<String> keys = new();
        private readonly HashSet<String> values = new();

        [Given(@"User parses the input Excel file from Data folder")]
        public void GivenUserParsesTheInputExcelFileFromDataFolder()
        {
            string filePath = "./Data/Excel/CountryCapitals.xlsx";
            excelResult = Functions.parseExcel(filePath);
        }

        [When(@"User gets the data present in the Excel sheet")]
        public void WhenUserGetsTheDataPresentInTheExcelSheet()
        {
            var collection = excelResult;
            if (collection != null)
            {
                for (int i = 0; i < collection.Count(); i++)
                {
                    Dictionary<String, String> dict = collection.ElementAt(i);
                    foreach (KeyValuePair<String, String> kvp in dict)
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
