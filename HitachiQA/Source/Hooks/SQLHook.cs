using BoDi;
using DocumentFormat.OpenXml.Bibliography;
using HitachiQA.Helpers;
using Microsoft.Extensions.Configuration;
using TechTalk.SpecFlow;

namespace HitachiQA.Hooks
{
    [Binding]
    public class SQLHook : HookBase
    {

        public SQLHook(IObjectContainer oc, FeatureContext fc, IConfiguration config) : base(oc, fc, config)
        {

        }

        [BeforeScenario]
        public void initialize()
        {
            Console.WriteLine("Attempting to load SQL Client");
            var connectionString = Configuration.GetVariable("SQL_CONNECTION_STRING", true);
            if(connectionString != null)
            {
                var client = new SQL(connectionString);
                ObjectContainer.RegisterInstanceAs<SQL>(client);
                Console.WriteLine("Loaded SQL Client");
            }
            else
            {
                Console.WriteLine("No SQL Client Loaded");
            }

        }

        [AfterScenario]
        public void tearDown()
        {
            if(ObjectContainer.IsRegistered<SQL>())
            {
                //ObjectContainer.Resolve<SQL>().Dispose();
            }
            
        }

    }
}
