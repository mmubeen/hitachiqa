using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Core;

namespace HitachiQA.Helpers
{
    public class FabricSQL
    {
        private readonly string _connectionString;

        public FabricSQL(string connectionString)
        {
            _connectionString = connectionString;
        }

        private async Task<DbConnection> GetConnectionAsync()
        {
            var cred = new DefaultAzureCredential();
            var token = await cred.GetTokenAsync(
                new TokenRequestContext(["https://database.windows.net/.default"])
                );
            var conn =  new SqlConnection(_connectionString);
            conn.AccessToken = token.Token;
            return conn;
        }

        public async Task<List<Dictionary<string, dynamic>>> ExecuteQueryAsync(string query)
        {
            using var connection = await GetConnectionAsync();
            var command = new SqlCommand(query, (SqlConnection)connection);
            var results = new List<Dictionary<string, dynamic>>();
            
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, dynamic>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                results.Add(row);
            }
            return results;
        }
    }

    public class SqlConnectionProviderusingMFA
    {
        private readonly string _connectionString;
        public SqlConnectionProviderusingMFA(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<DbConnection> GetConnectionAsync()
        {
            var cred = new DefaultAzureCredential();
            var token = await cred.GetTokenAsync(
                new TokenRequestContext(["https://database.windows.net/.default"])
                );
            var conn = new SqlConnection(_connectionString);
            conn.AccessToken = token.Token;
            return conn;
        }
    }

    public class SqlConnectionProviderusingUsingSP
    {
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _connectionString;

        public SqlConnectionProviderusingUsingSP(string connectionString)
        {
            _connectionString = connectionString;
            _tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            _clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            _clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
        }

        public async Task<DbConnection> GetConnectionAsync()
        {
            var cred = new ClientSecretCredential(_tenantId, _clientId, _clientSecret);
            var token = await cred.GetTokenAsync(
                new TokenRequestContext(["https://database.windows.net/.default"])
                );
            var conn = new SqlConnection(_connectionString);
            conn.AccessToken = token.Token;
            return conn;
        }
    }

    public class SqlConnectionProviderusingUsingMI
    {
        private readonly string _clientId;
        private readonly string _connectionString;

        public SqlConnectionProviderusingUsingMI(string connectionString)
        {
            _connectionString = connectionString;
            _clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        }

        public async Task<DbConnection> GetConnectionAsync()
        {
            var cred = new ManagedIdentityCredential();
            var token = await cred.GetTokenAsync(
                new TokenRequestContext(["https://database.windows.net/.default"])
                );
            var conn = new SqlConnection(_connectionString);
            conn.AccessToken = token.Token;
            return conn;
        }
    }
}