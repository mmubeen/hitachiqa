using HitachiQA;
using HitachiQA.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fabrics.Test.StepDefinitions
{
    public class QueryConfig
    {
        public string Table { get; set; }
        public string SQLDB2 { get; set; }
        public string SQLFDL { get; set; }
    }

    [Binding]
    public class DataTestingStepDefinitions
    {
        [Given("We access the data from DW Tables")]
        public async Task GivenWeAccessTheDataFromDWTables()
        {
            try
            {
                Log.Info("Starting database comparison from JSON configuration");

                // Load JSON file
                var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\.."));
                var jsonFilePath = Path.Combine(projectRoot, "query_config.json");
                if (!File.Exists(jsonFilePath))
                    throw new FileNotFoundException($"JSON configuration file not found at {jsonFilePath}");

                string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var queryConfigs = JsonSerializer.Deserialize<List<QueryConfig>>(jsonContent);

                if (queryConfigs == null || !queryConfigs.Any())
                    throw new Exception("No configurations found in the JSON file.");

                // Setup DB connections
                var db2Name = "DB2";
                var fdlName = "DB3";

                var db2Conn = $"Server=(localdb)\\MSSQLLocalDB;Database={db2Name};Trusted_Connection=True;";
                var fdlConn = $"Server=(localdb)\\MSSQLLocalDB;Database={fdlName};Trusted_Connection=True;";

                var db2 = new SQL(db2Conn);
                var fdl = new SQL(fdlConn);

                string outputDir = Path.Combine(projectRoot, "DW_Comparison_CSVs");
                Directory.CreateDirectory(outputDir);

                foreach (var config in queryConfigs)
                {
                    Log.Info($"Comparing table: {config.Table}");

                    var res2 = await db2.ExecuteQueryAsync(config.SQLDB2);
                    var resFdl = await fdl.ExecuteQueryAsync(config.SQLFDL);

                    var commonCols = (res2.FirstOrDefault()?.Keys.ToList() ?? new List<string>())
                     .Intersect(resFdl.FirstOrDefault()?.Keys.ToList() ?? new List<string>())
                     .ToList();


                    if (!commonCols.Any())
                    {
                        string file = Path.Combine(outputDir, $"{config.Table}_no_common_columns.csv");
                        await File.WriteAllTextAsync(file, $"No common columns for table '{config.Table}'");
                        continue;
                    }

                    string filePath = Path.Combine(outputDir, $"{config.Table}_comparison.csv");
                    using var writer = new StreamWriter(filePath);

                    // Header Info
                    writer.WriteLine($"First DB:, {db2Name}");
                    writer.WriteLine($"Second DB:, {fdlName}");
                    writer.WriteLine($"Query DB2:, {config.SQLDB2}");
                    writer.WriteLine($"Query FDL:, {config.SQLFDL}");
                    writer.WriteLine();

                    // Column Headers
                    var header = new List<string> { "Record #" };
                    foreach (var col in commonCols)
                    {
                        header.Add($"DB2.{col}");
                        header.Add($"FDL.{col}");
                        header.Add($"Mismatch ({col})");
                    }
                    writer.WriteLine(string.Join(",", header));

                    // Data Rows
                    int maxRows = Math.Max(res2.Count, resFdl.Count);
                    for (int i = 0; i < maxRows; i++)
                    {
                        bool rowMismatch = false;
                        foreach (var col in commonCols)
                        {
                            var val2 = i < res2.Count ? res2[i][col]?.ToString()?.Trim() ?? "NULL" : "[No Row]";
                            var valFdl = i < resFdl.Count ? resFdl[i][col]?.ToString()?.Trim() ?? "NULL" : "[No Row]";
                            if (val2 != valFdl) { rowMismatch = true; break; }
                        }

                        if (!rowMismatch) continue;

                        var row = new List<string> { (i + 1).ToString() };
                        foreach (var col in commonCols)
                        {
                            var val2 = i < res2.Count ? res2[i][col]?.ToString()?.Trim() ?? "NULL" : "[No Row]";
                            var valFdl = i < resFdl.Count ? resFdl[i][col]?.ToString()?.Trim() ?? "NULL" : "[No Row]";
                            var mismatch = val2 != valFdl ? "TRUE" : "FALSE";
                            row.Add(val2);
                            row.Add(valFdl);
                            row.Add(mismatch);
                        }

                        writer.WriteLine(string.Join(",", row.Select(EscapeForCsv)));
                    }
                }

                Log.Info($"CSV files saved to: {outputDir}");
            }
            catch (Exception ex)
            {
                Log.Error($"Comparison failed: {ex.Message}");
                throw;
            }
        }

        private string EscapeForCsv(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
