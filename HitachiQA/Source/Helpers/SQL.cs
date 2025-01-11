using Microsoft.Extensions.Configuration;
using System.Collections;
using Microsoft.Data.SqlClient;
using System.Text;

namespace HitachiQA.Helpers
{
    public class SQL
    {

        private readonly string _connectionString;
        public SQL(string ConnectionString)
        {
            _connectionString = ConnectionString;

        }
        public async Task<List<Dictionary<String, dynamic>>> ExecuteQueryAsync(String query)
        {
            return await ExecuteQueryAsync(query, ("", ""));
        }

        public async Task<List<Dictionary<String, dynamic>>> ExecuteQueryAsync(String query, params (string key, dynamic value)[] parameters)
        {
            var command = BuildExecuteQueryCommand(ref query, ref parameters);
            return await ExecuteQueryAsync(command);
        }



        public async Task<List<Dictionary<String, dynamic>>> ExecuteQueryAsync(SqlCommand command)
        {
            using var connection = new SqlConnection(_connectionString);
            command.Connection = connection;
            var results = new List<Dictionary<string, dynamic>>();

            await connection.OpenAsync();
            SqlDataReader reader = null;
            try
            {
                if (command.CommandText.StartsWith("insert", System.StringComparison.OrdinalIgnoreCase))
                {
                    var id = command.ExecuteScalarAsync();
                    results.Add(new Dictionary<string, dynamic>() { { "Id", Convert.ToInt64(id) } });
                    return results;
                }
                reader = await command.ExecuteReaderAsync();

            }
            catch (Exception ex)
            {
                throw new Exception($"Query: \n{command.CommandText}\n", ex);
            }

            try
            {
                while (await reader.ReadAsync())
                {
                    results.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(keyIndex => reader.GetName(keyIndex), valueIndex => reader.GetValue(valueIndex)));

                }
                return results;
            }
            finally
            {
                // Always call Close when done reading.
                await reader.CloseAsync();
            }

        }




        public async Task<long> InsertAsync(string tableName, params (string key, dynamic value)[] parameters)
        {
            return await InsertAsync(tableName, null, parameters);
        }
        public async Task<long> InsertAsync(string tableName, String sequenceName, params (string key, dynamic value)[] parameters)
        {
            var command = BuildInsertSqlCommand(tableName, sequenceName, parameters);
            var result = await ExecuteQueryAsync(command);
            var newId = (long)result.First()["Id"];
            await ExecuteQueryAsync($"UPDATE {tableName} SET LogicId = 'L'+@newIdStr where Id = @newId", ("@newId", newId), ("@newIdStr", newId.ToString()));
            return newId;

        }

        public static SqlCommand BuildExecuteQueryCommand(ref string query, ref (string key, dynamic value)[] parameters)
        {
            ProcessListParameterValues(ref query, ref parameters);

            var command = new SqlCommand(query);

            foreach (var parameter in parameters)
            {
                if (!(parameter.value is IEnumerable<object>))
                {
                    command.Parameters.AddWithValue(!parameter.key.StartsWith('@') ? '@' + parameter.key : parameter.key, parameter.value);
                }
            }

            return command;
        }

        public static void ProcessListParameterValues(ref string query, ref (string key, dynamic value)[] parameters)
        {
            //if a list of values is passed for a single parameter key, then we will do @key1, @key2 => (@key1, value1, @key2, value2)
            IEnumerable<(string key, dynamic value)> listParams = parameters.ToList().FindAll(param => !(param.value is string) && param.value is IEnumerable);
            if (listParams.Any())
            {
                foreach (var listParam in listParams)
                {
                    var str = new StringBuilder();
                    var i = 0;

                    foreach (var item in listParam.value)
                    {
                        str.Append(listParam.key + i + ", ");
                        parameters = parameters.Append((listParam.key + i, item)).ToArray();
                        i++;
                    }
                    var paramKeys = str.ToString().Trim().Trim(',');
                    query = query.Replace(listParam.key, paramKeys);
                }
            }
        }

        public static SqlCommand BuildInsertSqlCommand(string tableName, string sequenceName, (string key, dynamic value)[] parameters)
        {
            var statement = new StringBuilder();
            statement.Append($"INSERT INTO {tableName} (");
            if (sequenceName != null)
            {
                statement.Append(" Id, ");
            }
            statement.Append("InsertDateTime, insertedBy,SourceSystemId, Version, ");
            statement.Append(string.Join(", ", parameters.Select(it => it.key)));
            statement.Append(") OUTPUT INSERTED.Id VALUES (");
            if (sequenceName != null)
            {
                statement.Append($" NEXT VALUE FOR {sequenceName}, ");
            }
            statement.Append("GETDATE(), 'AutomationDataStubber', 0, FORMAT(GETDATE(), 'yyyyMMddHHmmssfff'), ");
            statement.Append(string.Join(", ", parameters.Select(it => $"@{it.key}")));
            statement.Append(");");

            var command = new SqlCommand(statement.ToString());
            foreach (var param in parameters)
            {
                var key = param.key;
                var val = param.value;
                if (!key.StartsWith("@"))
                    key = "@" + param.key;

                command.Parameters.AddWithValue(key, val);
            }

            return command;
        }



    }
}

