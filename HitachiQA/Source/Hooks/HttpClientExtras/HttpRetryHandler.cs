using Polly;

namespace HitachiQA.Hooks.HttpClientExtras
{
    public class HttpRetryHandler : DelegatingHandler
    {
        public HttpRetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var retry = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(1));
            var response = await retry.ExecuteAsync(async () => await base.SendAsync(request, cancellationToken));
            return response;
        }
    }
}
