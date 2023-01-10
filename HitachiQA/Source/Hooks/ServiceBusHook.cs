using BoDi;
using DocumentFormat.OpenXml.Bibliography;
using HitachiQA.Helpers;
using Microsoft.Extensions.Configuration;
using TechTalk.SpecFlow;

namespace HitachiQA.Hooks
{
    [Binding]
    public class ServiceBusHook : HookBase
    {

        public ServiceBusHook(IObjectContainer oc, FeatureContext fc, IConfiguration config) : base(oc, fc, config)
        {

        }

        [BeforeScenario]
        public void initialize()
        {
            Console.WriteLine("Attempting to load Service Bus Client");
            var SrvcBusUri = Configuration.GetVariable("SERVICE_BUS_NAMESPACE_URI", true);
            if(Main.IsValid(SrvcBusUri))
            {
                SrvcBusUri.NullGuard();
                var client = new ServiceBus(SrvcBusUri);
                ObjectContainer.RegisterInstanceAs<ServiceBus>(client);
                Console.WriteLine("Loaded Service Bus Client");

            }else{ Console.WriteLine("No Service Bus Client Loaded"); }

        }

        [AfterScenario]
        public void tearDown()
        {
            if(ObjectContainer.IsRegistered<Cosmos>())
            {
                ObjectContainer.Resolve<Cosmos>().Dispose();
            }
            
        }

    }
}
