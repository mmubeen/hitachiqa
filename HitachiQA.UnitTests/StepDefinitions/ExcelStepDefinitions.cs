using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HitachiQA.Helpers;
using Microsoft.Extensions.Configuration;

namespace HitachiQA.UnitTests.StepDefinitions
{
    [Binding]
    public class ExcelStepDefinitions
    {
        
        private IEnumerable<Dictionary<String, String>> excelResult;
        private HashSet<String> keys = new HashSet<String>();
        private HashSet<String> values = new HashSet<String>();



        [Given(@"User parses the input Excel file from Data folder")]
        public void GivenUserParsesTheInputExcelFileFromDataFolder()
        {
            string filePath = "./Data/Excel/CountryCapitals.xlsx";
            excelResult = Functions.parseExcel(filePath);
        }

        [When(@"User gets the data present in the Excel sheet")]
        public void WhenUserGetsTheDataPresentInTheExcelSheet()
        {
            IEnumerable<Dictionary<String, String>> collection = excelResult;
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

        [Then(@"User validates the data is parsed correctly")]
        public void ThenUserValidatesTheDataIsParsedCorrectly()
        {
            HashSet<String> Inputkeys = new HashSet<String>()
            {
                "Country","Capital"
            };

            HashSet<String> InputValues = new HashSet<String>()
            {
                "Canada" ,"Ottawa",
                "India"  , "Delhi",
                "USA"    , "Washington DC"
            };


            (Inputkeys.Intersect(keys).Count() == Inputkeys.Count).Should().BeTrue();

            

            (InputValues.Intersect(values).Count() == InputValues.Count).Should().BeTrue();

        }
    }
}
