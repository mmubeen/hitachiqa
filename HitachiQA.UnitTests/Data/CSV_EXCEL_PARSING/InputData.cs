using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.UnitTests.Data.CSV_EXCEL_PARSING
{
    public static class InputData
    {
        public static readonly HashSet<string> ExcelInputKeys = new()
            {
                "Country","Capital"
            };

        public static readonly HashSet<string> ExcelInputValues = new()
            {
                "Canada" ,"Ottawa",
                "India"  , "Delhi",
                "USA"    , "Washington DC"
            };

        public static readonly List<string> CSVInputKVP = new()
        {
            "Key: FirstName, Value: John",
            "Key: LastName, Value: Doe",
            "Key: Address, Value: 120 jefferson st.",
            "Key: City, Value: NJ",
            "Key: FirstName, Value: Jack",
            "Key: LastName, Value: McGinnis",
            "Key: Address, Value: 220 hobo Av.",
            "Key: City, Value: PA",
            "Key: FirstName, Value: Mike",
            "Key: LastName, Value: Keith",
            "Key: Address, Value: 120 Jefferson St.",
            "Key: City, Value: NJ",
            "Key: FirstName, Value: Stephen",
            "Key: LastName, Value: Tyler",
            "Key: Address, Value: 7452 Terrace",
            "Key: City, Value: SD"
        };
    }
}
