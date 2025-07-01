using System;
using HitachiQA;
using IBM.Data.Db2;

using Reqnroll;

namespace Fabrics.Test.StepDefinitions
{

    [Binding]
    public class DataTestingStepDefinitions

    {

        [Given("We access the data from DW Tables")]
        public async Task GivenWeAccessTheDataFromDWTables()
        {

            Log.Info("Starting: DW table access");

            var host = Environment.GetEnvironmentVariable("DB_HOST");
            var port = Environment.GetEnvironmentVariable("DB_PORT");
            var name = Environment.GetEnvironmentVariable("DB_NAME");
            var user = Environment.GetEnvironmentVariable("DB_USER");
            var pass = Environment.GetEnvironmentVariable("DB_PASS");

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(port) ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(pass))
            {
                throw new Exception("One or more required DB environment variables are missing or empty.");
            }

            string connectionString = $"Server={host}:{port};" +
                                      $"Database={name};" +
                                      $"UserID={user};" +
                                      $"Password={pass};" +
                                      $"Connect Timeout=30;";

            try

            {

                using var connection = new DB2Connection(connectionString);

                Log.Info("Opening DB connection...");

                await connection.OpenAsync();

                Log.Info("DB connection successful!");

                string sql = "SELECT CURRENT TIMESTAMP FROM SYSIBM.SYSDUMMY1";

                using var command = new DB2Command(sql, connection);

                var result = await command.ExecuteScalarAsync();

                if (result == null)

                    throw new Exception("No result returned from DB query.");

                Log.Info($"Current DB2 Timestamp: {result}");

                await ExecuteExampleQueryAsync(connection);

            }

            catch (DB2Exception ex)

            {

                throw new Exception($"DB2 Error: {ex.Message} | SQL State: {ex.SqlState} | Error Code: {ex.ErrorCode}");

            }

            catch (Exception ex)

            {

                throw new Exception($"General Error: {ex.Message}");

            }

        }

        private async Task ExecuteExampleQueryAsync(DB2Connection connection)

        {

            string sql = @"
                 
                SELECT TABSCHEMA, TABNAME 
   
                FROM SYSCAT.TABLES 
    
                WHERE TABSCHEMA NOT LIKE 'SYS%' 
    
                FETCH FIRST 10 ROWS ONLY";

            Log.Info("Executing table list query...");

            using var command = new DB2Command(sql, connection);

            using var reader = await command.ExecuteReaderAsync();

            int rowCount = 0;

            Log.Info("Schema\t\tTable Name");

            Log.Info("------\t\t----------");

            while (await reader.ReadAsync())

            {

                string schema = reader["TABSCHEMA"].ToString();

                string tableName = reader["TABNAME"].ToString();

                Log.Info($"{schema}\t\t{tableName}");

                rowCount++;

            }

            if (rowCount == 0)

                throw new Exception("No user tables found in SYSCAT.TABLES.");

            else

                Log.Info($"Retrieved {rowCount} user tables.");

        }

    }

}
