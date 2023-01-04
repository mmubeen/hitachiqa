using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using HitachiQA.Driver;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using System.Diagnostics.CodeAnalysis;
using BoDi;
using OpenQA.Selenium.DevTools;

namespace HitachiQA.Helpers
{
    public class Functions
    {
        private RestAPI RestAPI;
        public Functions(ObjectContainer objectContainer)
        {
            this.RestAPI = objectContainer.Resolve<RestAPI>();

        }

        public string GetRandomVIN()
        {
            //grabs random vin via randomvin.com
            string vin;
            try
            {
                vin = (string)RestAPI.GET("https://randomvin.com/getvin.php?type=real");
                if (string.IsNullOrWhiteSpace(vin))
                {
                    return GetRandomVIN();
                }
                return vin;
            }
            catch
            {
                return GetRandomVIN();
            }
        }

       

        public static string ParseURL(string URL_OR_PATH, params (string key, string value)[] parameters)
        {
            foreach (var param in parameters)
            {
                URL_OR_PATH = URL_OR_PATH.Replace("{" + param.key + "}", param.value);
            }
            return ParseURL(URL_OR_PATH);
        }

        public static string ParseURL(string URL_OR_PATH)
        {
            if (URL_OR_PATH.StartsWith("http"))
            {
                return URL_OR_PATH;
            }
            else
            {
                return ParseURL(Main.Configuration["Host"], URL_OR_PATH);
            }
        }

        public static string ParseURL(string Host, string Path)
        {
            return Host + (Path.StartsWith('/') ? Path : "/" + Path);
        }

        //
        //  Failure Handling
        //
        public static System.Exception handleFailure(string message, Exception ex = null, bool optional = false)
        {
            if (optional)
            { Log.Info(message); }
            else
            {
                Log.Error(message);
                if (ex != null)
                {
                    handleFailure(ex, optional);
                }
                else
                {
                    throw new Exception(message);
                }
            }
            return ex ?? new Exception(message);
        }

        public static System.Exception handleFailure(string message, Exception ex = null, bool optional = false, params (string key, dynamic value)[] parameters)
        {
            if (optional)
            { Log.Info(message, parameters); }
            else
            {
                Log.Error(message, parameters);
                if (ex != null)
                {
                    handleFailure(ex, optional);
                }
                else
                {
                    throw new Exception(message);
                }
            }
            return ex ?? new Exception(message);
        }

        public static System.Exception handleFailure(Exception ex, bool optional = false)
        {
            if (optional)
            {
                Log.Info(ex.Message);
            }
            else
            {
                Log.Error(ex.Message);
                throw ex;
            }
            return ex;
        }

        public static Dictionary<string, string> TableToDictionary(TechTalk.SpecFlow.Table table)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var row in table.Rows)
            {
                dictionary.Add(row[0], row[1]);
            }
            return dictionary;
        }

        public static IEnumerable<Dictionary<String, String>> parseExcel(String filePath, int headerRow = 0)
        {
            var tasks = new List<Task<Dictionary<String, String>>>();
            try
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filePath, false))
                {
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                    Row[] sheetData = worksheetPart.Worksheet.Elements<SheetData>().First().Elements<Row>().ToArray<Row>();

                    var header = sheetData[0].Elements<Cell>().Select(cell => extractCellText(workbookPart, cell)).ToArray<string>();

                    for (int rowIndex = 1; rowIndex < sheetData.Length; rowIndex++)
                    {
                        tasks.Add(parseRow(workbookPart, header, sheetData[rowIndex], filePath));
                    }
                }
                return tasks.Select(it => it.Result);
            }
            catch(Exception ex)
            {
                Log.Debug($"File-> {filePath}");
                throw ex;
            }
        }

        public static List<Dictionary<String, String>> parseCSV(String filePath, int headerRow = 0)
        {
            List<string> header = new List<String>();

            if( !File.Exists(filePath))
            {
                handleFailure($"File {filePath} does not exist");
            }
            filePath = Path.IsPathFullyQualified(filePath) ? filePath : Path.GetFullPath(filePath);

            Log.Debug(filePath);

            List<Dictionary<String, String>> result = new List<Dictionary<String, String>>();

            using (TextReader reader = new StreamReader(filePath))
            {
                var config = new CsvConfiguration(CultureInfo.CreateSpecificCulture("en-US"))
                {
                    MissingFieldFound = null,
                };
                using (var csvReader = new CsvReader(reader, config))
                {
                    // calculate number of columns
                    csvReader.Read();
                    int i = 0;

                    var columnName = csvReader.GetField(i);

                    while (!String.IsNullOrEmpty(columnName))
                    {
                        header.Add(columnName);
                        i++;
                        columnName = csvReader.GetField(i);
                    }

                    while (csvReader.Read())
                    {
                        Dictionary<String, String> row = new Dictionary<String, String>();
                        for(int col =0; col < header.Count; col++)
                        {
                            row.Add(header.ElementAt(col), csvReader.GetField(col));
                        }
                        result.Add( row);
                    }
                }
            }
            return result;
        }

        private static async Task<Dictionary<String, String>> parseRow(WorkbookPart workbookPart, string [] header, Row row, string filePath)
        {
            var cells = row.Elements<Cell>().ToArray<Cell>();

            var dict = new Dictionary<String, String>();
            for (int i = 0; i < header.Length; i++)
            {
                Cell cell;
                try
                {
                     cell= cells[i];
                }
                catch(IndexOutOfRangeException)
                {
                    cell = new Cell();
                }
                try
                {
                    dict.Add(header[i], extractCellText(workbookPart, cell));
                }
                catch(Exception ex)
                {
                    Log.Debug($"File-> {filePath}");
                    throw ex;
                }
            }
            return dict;
        }

        private static String extractCellText(WorkbookPart workbookPart, Cell cell)
        {
            var cellValue = cell.CellValue;
            var text = (cellValue == null) ? cell.InnerText : cellValue.Text;
            if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
            {
                text = workbookPart.SharedStringTablePart.SharedStringTable
                    .Elements<SharedStringItem>().ElementAt(
                        Convert.ToInt32(cell.CellValue.Text)).InnerText;
            }
            return (text ?? string.Empty).Trim();
        }

        public static dynamic parseRatingFactorNumericalValues(String value)
        {
            if (value.Contains("+"))
            {
                return int.MaxValue;
            }
            else if(int.TryParse(value, out int intValue))
            {
                return intValue;
            }
            else
            {
                return null;
            }
        }

        public static int parseInt(string IntegerString)
        {
            try
            {
                return int.Parse(string.Join("", IntegerString.Where(Char.IsDigit)));
            }
            catch (Exception ex)
            {
                throw handleFailure($"String {IntegerString} Failed to parse into int", ex);
            }
        }

        public static string GetValidDriverLicense(string state)
        {
            //not a great implementation but it works

            char[] chars =  "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            Random r = new Random();

            string licenseNo = "";
            
            if(state.ToUpper() == "IL")
            {
                licenseNo = ""+chars[r.Next(chars.Length)];
            }

            licenseNo += (r.Next(100, 1000).ToString()) + (r.Next(1000, 10000).ToString()) + (r.Next(1000, 10000).ToString());

            return licenseNo;
        }

        public static string getValidCreditCardNumber(string cardType="Visa")
        {
            switch (cardType)
            {
                case "AmericanExpress":
                    return "370000000000002";

                case "Discover":
                    return "6011000000000012";

                case "JCB":
                    return "3088000000000017";
           
                case "Visa":
                    return "4111111111111111";

                case "Visa1":
                    return "4012888818888";

                case "Visa2":
                    return "4007000000027";

                case "Mastercard":
                    return "5424000000000015";

                case "Mastercard1":
                    return "2223000010309703";

                case "Mastercard2":
                    return "2223000010309711";

                case "DinersClub/CarteBlanche":
                    return "38000000000006";

                default:
                    throw handleFailure($"Credit Card Type: {cardType} is not supported");
            }
        }

        public static int GetRandomInteger(int max=100)
        {
           return new Random().Next(max);  
        }
      
       
        public static string EncryptString(string plainText)
        {
            return Cryptography.Encrypt(plainText);
        }

        public static string DecryptString(string encryptedText)
        {
            return Cryptography.Decrypt(encryptedText);
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static decimal CalculateSimilarityPercent(string s, string t)
        {
            if(string.IsNullOrWhiteSpace(s) || string.IsNullOrWhiteSpace(t)) return 0;
            if (s == t) return 1;

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
                return m;
            

            if (m == 0)
                return n;
            
            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }
            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            var distance =  d[n, m];
            return (1.0M - ((decimal)distance / (decimal)Math.Max(s.Length, t.Length)));

        }
    }
}
