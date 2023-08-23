using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace HitachiQA.Source.Hooks.HttpClientExtras
{
    public class HttpLoggingHandler : DelegatingHandler
    {
        public HttpLoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();
            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        var content = response.RequestMessage.Content;
                        if (content != null)
                        {
                            Write($"\nBody:" + JToken.Parse(await response.RequestMessage.Content.ReadAsStringAsync()));
                        }
                    }
                    catch (Exception) { }
                }
            }
            finally
            {
                timer.Stop();
                Write($"\nRequest=>[{request.Method.Method}] {request.RequestUri} took: {timer.Elapsed.TotalSeconds:0.00} Seconds\n");

            }
            return response;
        }
        private void Write(string msg)
        {
            Debug.Write(msg);
        }
    }
}
