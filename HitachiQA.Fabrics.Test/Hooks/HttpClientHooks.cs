using Microsoft.Extensions.Configuration;
using Reqnroll.BoDi;
using Microsoft.Fabric.Api;
using HitachiQA.HttpClients;


namespace HitachiQA.Fabrics.Test.Hooks
{
    [Binding]
    public class HttpClientHooks
    {
        private readonly IConfiguration config;

        public HttpClientHooks(IConfiguration config)
        {
            this.config = config;
        }

        [BeforeFeature]
        public static void InitializeClients(
            IObjectContainer ioc, 
            AuthorizationClient ac)
        {
            var fabricsClient = new FabricClient(ac);
            ioc.RegisterInstanceAs(fabricsClient);

        }
    }
}
