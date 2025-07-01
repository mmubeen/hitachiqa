using Microsoft.Extensions.Configuration;
using Reqnroll.BoDi;
using Microsoft.Fabric.Api;
using Fabrics.Test.Orchestrators.ExpectedProviders;
using HitachiQA.HttpClients;


namespace HitachiQA.Fabrics.Test.Hooks
{
    [Binding]
    public class ExpectedProviderHook
    {
        private readonly IConfiguration config;

        public ExpectedProviderHook(IConfiguration config)
        {
            this.config = config;
        }

        [BeforeFeature]
        public static void InitializeClients(
            IObjectContainer ioc, 
            AuthorizationClient ac)
        {
            ioc.RegisterTypeAs<GithubExpectedProvider, IExpectedProvider>();

        }
    }
}
