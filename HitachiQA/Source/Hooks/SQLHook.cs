using HitachiQA.Helpers;
using Microsoft.Extensions.Configuration;
using Reqnroll.BoDi;

namespace HitachiQA.Hooks
{
    [Binding]
    public class SQLHook : HookBase
    {

        public SQLHook(IConfiguration config) : base(config)
        {

        }

        [BeforeFeature]
        public static void initialize(IObjectContainer oc, IConfiguration config)
        {
            Console.WriteLine("Attempting to load SQL Client");
            var connectionString = config.GetVariable("SQL_CONNECTION_STRING", true);
            if (connectionString != null)
            {
                connectionString = connectionString.Replace(";ProviderName=system.data.sqlclient", "");
                var client = new SQL(connectionString);
                oc.RegisterInstanceAs<SQL>(client);
                Console.WriteLine("Loaded SQL Client");
            }
            else
            {
                Console.WriteLine("No SQL Client Loaded");
            }
        }
    }
}
