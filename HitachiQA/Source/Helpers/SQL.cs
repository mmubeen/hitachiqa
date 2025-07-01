using Microsoft.Extensions.Configuration;
using System.Collections;
using Microsoft.Data.SqlClient;
using System.Text;
using IBM.Data.Db2;
using System.Data.Common;

namespace HitachiQA.Helpers
{
    public class SQL
    {

        private readonly string _connectionString;
        private bool _useDb2 { get; init; }
        public SQL(string ConnectionString, bool useDb2)
        {
            _connectionString = ConnectionString;
            _useDb2 = useDb2;
        }

        private DbConnection GetConnection()
        {
            if (_useDb2)
                return new DB2Connection(_connectionString);

            return new SqlConnection(_connectionString);
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

        public async Task<List<Dictionary<String, dynamic>>> ExecuteQueryAsync(DbCommand command)
        {
            using var connection = GetConnection();
            command.Connection = connection;
            var results = new List<Dictionary<string, dynamic>>();

            await connection.OpenAsync();
            DbDataReader reader = null;
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

        public DbCommand BuildExecuteQueryCommand(ref string query, ref (string key, dynamic value)[] parameters)
        {
            ProcessListParameterValues(ref query, ref parameters);

            if (_useDb2)
            {
                var command = new DB2Command(query);

                foreach (var parameter in parameters)
                {
                    if (!(parameter.value is IEnumerable<object>) || parameter.value is string)
                    {
                        var param = new DB2Parameter
                        {
                            ParameterName = parameter.key.StartsWith("@") ? parameter.key : "@" + parameter.key,
                            Value = parameter.value ?? DBNull.Value
                        };
                        command.Parameters.Add(param);
                    }
                }

                return command;
            }
            else
            {
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


    }
}

