using HitachiQA.Helpers;
using Microsoft.Extensions.Configuration;
using Reqnroll.BoDi;
 
namespace HitachiQA.Hooks

{
    [Binding]
    public class FabricSQLHook : HookBase

    {
        public FabricSQLHook(IConfiguration config) : base(config)

        {

        }

        [BeforeFeature]

        public static void Initialize(IObjectContainer oc, IConfiguration config)

        {
            Console.WriteLine("Attempting to load Fabric SQL Client");

            Console.WriteLine("FABRIC_USE = " + config.GetValue<string>("FABRIC_USE"));
            Console.WriteLine("FABRIC_CONNECTION_STRING = " + config.GetValue<string>("FABRIC_CONNECTION_STRING"));

            var connectionString = config.GetVariable("FABRIC_CONNECTION_STRING", true);
            var useFabric = config.GetValue<bool>("FABRIC_USE");

            if (useFabric && !string.IsNullOrWhiteSpace(connectionString))

            {
                var client = new FabricSQL(connectionString);

                oc.RegisterInstanceAs<FabricSQL>(client);

                Console.WriteLine("Loaded Fabric SQL Client");

            }

            else

            {

                Console.WriteLine("FABRIC_USE is not set to true, skipping Fabric client.");

            }

        }

    }

}
