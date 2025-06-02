using HitachiQA.Fabrics.Test.Client.HttpClients;
using HitachiQA.Source.Hooks.HttpClientExtras;
using HitachiQA.Source.HttpClients;
using Microsoft.Extensions.Configuration;
using Reqnroll.BoDi;
using System.Net.Http.Headers;

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
            AuthorizationClient ac,
            IConfiguration config)
        {
            var socketHander = new SocketsHttpHandler()
            {
                MaxConnectionsPerServer = 10
            };
            var logging = new HttpLoggingHandler(socketHander);
            var auth = new HttpAuthHandler(logging, ac, config);
            var retry = new HttpRetryHandler(auth);
            var client = new HttpClient(retry);
            var host = config.GetVariable("SERVER_HOST");
            client.BaseAddress = new Uri(host);
            client.Timeout = TimeSpan.FromSeconds(120);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            var fabricsClient = new FabricsClient(client);
            ioc.RegisterInstanceAs(fabricsClient);

        }
    }
}
