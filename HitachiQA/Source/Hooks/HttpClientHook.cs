using HitachiQA.Helpers;
using HitachiQA.Hooks;
using HitachiQA.Hooks.HttpClientExtras;
using HitachiQA.HttpClients;
using Microsoft.Extensions.Configuration;
using Reqnroll.BoDi;
using System.Net.Http.Headers;

namespace HitachiQA.Hooks
{
    public class HttpClientHook : HookBase
    {
        public HttpClientHook(IConfiguration config) : base(config)
        {

        }

        [BeforeFeature]
        public static void initialize(IObjectContainer oc, IConfiguration config, AuthorizationClient authClient)
        {
            var apiClient = BuildClient<RestAPI>(config, "SERVER_HOST", authClient);
            oc.RegisterInstanceAs(apiClient);

        }

        public static T BuildClient<T>(IConfiguration config, string baseUrlVarName, AuthorizationClient authClient)
        {
            var baseUrl = config.GetVariable(baseUrlVarName, true);
            if (baseUrl == null)
            {
                try
                {
                    baseUrl = config.GetVariable("SERVER_HOST");
                }
                catch (Exception ex)
                {
                    throw new Exception($"please populate 'SERVER_HOST' with restAPI endpoint\n", ex);
                }
            }


            var client = new HttpClient(GetHttpHandlers(authClient, config));
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(120);
            client.DefaultRequestHeaders.Add("User-Agent", "HitachiQA Automation");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            return (T)Activator.CreateInstance(typeof(T), client);
        }
        private static HttpMessageHandler GetHttpHandlers(AuthorizationClient authClient, IConfiguration config)
        {
            var socketsHttpHandler = new SocketsHttpHandler()
            {
                MaxConnectionsPerServer = 10
            };
            var logging = new HttpLoggingHandler(socketsHttpHandler);
            var auth = new HttpAuthHandler(logging, authClient, config);
            var retry = new HttpRetryHandler(auth);
            return retry;
        }
    }

}
