using HitachiQA;
using HitachiQA.Helpers;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fabrics.Test.StepDefinitions
{
    [Binding]
    public class DataTestingStepDefinitions
    {
        [Given("We access the data from DW Tables")]
        public async Task GivenWeAccessTheDataFromDWTables()
        {
            try
            {
                Log.Info("Starting database comparison");

                var db2Name = "DB2";
                var db3Name = "DB3";

                var db2Conn = $"Server=(localdb)\\MSSQLLocalDB;Database={db2Name};Trusted_Connection=True;";
                var db3Conn = $"Server=(localdb)\\MSSQLLocalDB;Database={db3Name};Trusted_Connection=True;";

                var db2 = new SQL(db2Conn);
                var db3 = new SQL(db3Conn);

                string tableQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                var tablesDB2 = (await db2.ExecuteQueryAsync(tableQuery))?.Select(r => (string)r["TABLE_NAME"]).ToList() ?? new List<string>();
                var tablesDB3 = (await db3.ExecuteQueryAsync(tableQuery))?.Select(r => (string)r["TABLE_NAME"]).ToList() ?? new List<string>();
                var allTables = new HashSet<string>(tablesDB2.Union(tablesDB3));

                string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "DW_Comparison_CSVs");
                Directory.CreateDirectory(outputDir);

                foreach (string table in allTables)
                {
                    Log.Info($"Comparing table: {table}");

                    if (!tablesDB2.Contains(table) || !tablesDB3.Contains(table))
                    {
                        string missingFile = Path.Combine(outputDir, $"{table}_missing_table.csv");
                        await File.WriteAllTextAsync(missingFile, $"Table '{table}' missing in {(tablesDB2.Contains(table) ? db3Name : db2Name)}");
                        continue;
                    }

                    var db2Cols = await GetTableColumns(db2, table);
                    var db3Cols = await GetTableColumns(db3, table);
                    var commonCols = db2Cols.Intersect(db3Cols).ToList();
                    var missingInDB2 = db3Cols.Except(db2Cols).ToList();
                    var missingInDB3 = db2Cols.Except(db3Cols).ToList();

                    if (!commonCols.Any())
                    {
                        string file = Path.Combine(outputDir, $"{table}_no_common_columns.csv");
                        await File.WriteAllTextAsync(file, $"No common columns in table '{table}'");
                        continue;
                    }

                    string sql = $"SELECT {string.Join(", ", commonCols.Select(c => $"[{c}]"))} FROM [{table}]";
                    var res2 = await db2.ExecuteQueryAsync(sql);
                    var res3 = await db3.ExecuteQueryAsync(sql);

                    string filePath = Path.Combine(outputDir, $"{table}_comparison.csv");
                    using var writer = new StreamWriter(filePath);

                    // Header Info
                    writer.WriteLine($"First DB:, {db2Name}");
                    writer.WriteLine($"Second DB:, {db3Name}");
                    writer.WriteLine($"Query Executed:, {sql}");
                    writer.WriteLine();

                    // Schema Differences
                    if (missingInDB2.Any() || missingInDB3.Any())
                    {
                        writer.WriteLine("Schema Differences:");
                        if (missingInDB2.Any())
                            writer.WriteLine($"Columns missing in {db2Name}:, {string.Join(", ", missingInDB2)}");
                        if (missingInDB3.Any())
                            writer.WriteLine($"Columns missing in {db3Name}:, {string.Join(", ", missingInDB3)}");
                        writer.WriteLine();
                    }

                    // Column Headers
                    var header = new List<string> { "Record #" };
                    foreach (var col in commonCols)
                    {
                        header.Add($"DB2.{col}");
                        header.Add($"DB3.{col}");
                        header.Add($"Mismatch ({col})");
                    }
                    writer.WriteLine(string.Join(",", header));

                    // Data Rows
                    int maxRows = Math.Max(res2.Count, res3.Count);
                    for (int i = 0; i < maxRows; i++)
                    {
                        bool rowMismatch = false;
                        foreach (var col in commonCols)
                        {
                            var val2 = i < res2.Count ? res2[i][col]?.ToString()?.Trim() ?? "NULL" : "[No Row]";
                            var val3 = i < res3.Count ? res3[i][col]?.ToString()?.Trim() ?? "NULL" : "[No Row]";
                            if (val2 != val3) { rowMismatch = true; break; }
                        }

                        if (!rowMismatch) continue;

                        var row = new List<string> { (i + 1).ToString() };
                        foreach (var col in commonCols)
                        {
                            var val2 = i < res2.Count ? res2[i][col]?.ToString()?.Trim() ?? "NULL" : "[No Row]";
                            var val3 = i < res3.Count ? res3[i][col]?.ToString()?.Trim() ?? "NULL" : "[No Row]";
                            var mismatch = val2 != val3 ? "TRUE" : "FALSE";
                            row.Add(val2);
                            row.Add(val3);
                            row.Add(mismatch);
                        }

                        writer.WriteLine(string.Join(",", row.Select(EscapeForCsv)));
                    }
                }

                Log.Info($"CSV files saved to: {outputDir}");
            }
            catch (Exception ex)
            {
                Log.Error("Comparison failed");
                throw;
            }
        }

        private async Task<List<string>> GetTableColumns(SQL db, string table)
        {
            string colQuery = $@"
                SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = '{table}'
                ORDER BY ORDINAL_POSITION";
            var rows = await db.ExecuteQueryAsync(colQuery);
            return rows?.Select(r => (string)r["COLUMN_NAME"]).ToList() ?? new List<string>();
        }

        private string EscapeForCsv(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
