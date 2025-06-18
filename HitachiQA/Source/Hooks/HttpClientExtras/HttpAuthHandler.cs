using HitachiQA.HttpClients;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;


namespace HitachiQA.Hooks.HttpClientExtras
{
    public class HttpAuthHandler : DelegatingHandler
    {
        private AuthorizationClient Client { get; init; }
        private IConfiguration Configuration { get; set; }
        public HttpAuthHandler(HttpMessageHandler innerHandler, AuthorizationClient client, IConfiguration config)
            : base(innerHandler)
        {
            Client = client;
            Configuration = config;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (isAuthRequired(request.RequestUri))
            {
                var bearer = await Client.GetBearerTokenAsync();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            }

            var response = await base.SendAsync(request, cancellationToken);

            return response;
        }

        public bool isAuthRequired(Uri requestUri)
        {
            if (requestUri == null)
            {
                return false;
            }
            var host = Configuration.GetVariable("SERVER_HOST", true);
            return host != null && new Uri(host).Host == requestUri?.Host;
        }
    }
}
