using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace HitachiQA.Hooks.HttpClientExtras
{
    public class HttpLoggingHandler : DelegatingHandler
    {
        public HttpLoggingHandler()
            : base(new HttpClientHandler())
        {
        }
        public HttpLoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();
            var uri = string.Empty;
            HttpResponseMessage response;
            try
            {
                uri = request.RequestUri.ToString();
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
                Write($"\nRequest=>[{request.Method.Method}] {uri} took: {timer.Elapsed.TotalSeconds:0.00} Seconds\n");

            }
            return response;
        }
        private void Write(string msg)
        {
            Debug.Write(msg);
        }
    }
}
