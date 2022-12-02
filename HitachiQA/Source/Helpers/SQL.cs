using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Linq;
using System.Collections;
using BoDi;
using HitachiQA.Hooks;
using Microsoft.Extensions.Configuration;

namespace HitachiQA.Helpers
{
    public class SQL
    {
        private readonly string ConnectionString;
        public SQL(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;

        }
        public List<Dictionary<String, dynamic>> executeQuery(String query)
        {
            return executeQuery(query, ("",""));
        }

        public List<Dictionary<String, dynamic>> executeQuery(String query, params (string key, dynamic value)[] parameters)
        {
            IEnumerable<(string key, dynamic value)> listParams = parameters.ToList().FindAll(param => !(param.value is string) && param.value is IEnumerable);
            if (listParams.Any())
            {
                foreach(var listParam in listParams)
                {
                    var str = new StringBuilder();
                    int i = 0;
 
                    foreach(var item in listParam.value)
                    {
                        str.Append(listParam.key + i + ", ");
                        parameters = parameters.Append((listParam.key + i, item)).ToArray();
                        i++;
                    }
                    var paramKeys = str.ToString().Trim().Trim(',');
                    query = query.Replace(listParam.key, paramKeys);
                }
            }

            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                foreach (var parameter in parameters)
                {
                    if (!(parameter.value is IEnumerable<object>))
                    {
                        command.Parameters.AddWithValue(parameter.key, parameter.value);
                    }
                }
                connection.Open();
                SqlDataReader reader = null;
                try
                {
                    reader = command.ExecuteReader();

                }
                catch (Exception ex)
                {
                    Functions.handleFailure($"Query: \n{query}\n", ex);
                }

                try
                {
                    List<Dictionary<String, dynamic>> results = new List<Dictionary<string, dynamic>>();
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
    }
}
