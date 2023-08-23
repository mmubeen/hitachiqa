using System.Text;
using System.Data.SqlClient;
using System.Collections;
using Microsoft.Extensions.Configuration;

namespace HitachiQA.Helpers
{
    public class SQL
    { 
    
        private string ConnectionString { get; init; }

        public IConfiguration Config { get; init; }

        public SQL(IConfiguration config, string connectionString)
        {
            Config = config;
            ConnectionString = connectionString;

        }
        public List<Dictionary<String, dynamic>> executeQuery(String query)
        {
            return executeQuery(query, ("", ""));
        }

        public List<Dictionary<String, dynamic>> executeQuery(String query, params (string key, dynamic value)[] parameters)
        {
            var command = BuildExecuteQueryCommand(ref query, ref parameters);
            return executeQuery(command);
        }


        public List<Dictionary<String, dynamic>> executeQuery(SqlCommand command)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                command.Connection = connection;
                var results = new List<Dictionary<string, dynamic>>();

                connection.Open();
                SqlDataReader reader = null;
                try
                {
                    if (command.CommandText.StartsWith("insert", System.StringComparison.OrdinalIgnoreCase))
                    {
                        var id = command.ExecuteScalar();
                        results.Add(new Dictionary<string, dynamic>() { { "Id", Convert.ToInt64(id) } });
                        return results;
                    }
                    reader = command.ExecuteReader();

                }
                catch (Exception ex)
                {
                    throw new Exception($"Query: \n{command.CommandText}\n", ex);
                }

                try
                {
                    while (reader.Read())
                    {
                        results.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(keyIndex => reader.GetName(keyIndex), valueIndex => reader.GetValue(valueIndex)));

                    }
                    return results;
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }
            }

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

    }
}

