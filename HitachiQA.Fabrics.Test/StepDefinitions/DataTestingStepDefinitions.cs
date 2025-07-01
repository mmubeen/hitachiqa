using HitachiQA;
using HitachiQA.Helpers;

namespace Fabrics.Test.StepDefinitions;


[Binding]
public class DataTestingStepDefinitions
{
    private readonly SQL db2;

    public DataTestingStepDefinitions(SQL sql)
    {
        db2 = sql;
    }


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

        string sql01 = @"SELECT CURRENCY_EXCHANGE_KEY, CURRENCY_KEY FROM ead_md.currency_exchange_dimension WHERE INSERT_DATE BETWEEN '2025-02-01' AND '2025-05-30' ORDER BY CURRENCY_EXCHANGE_KEY LIMIT 10";
        var result = await db2.ExecuteQueryAsync(sql01);
        Log.Info($"Returned {result.Count} results");
        Log.Info(result);


    }

}
