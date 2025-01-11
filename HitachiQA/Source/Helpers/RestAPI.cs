using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace HitachiQA.Helpers
{
    public class RestAPI
    {
        public List<(string key, double seconds)> timeSpans = new List<(string key, double seconds)>();
        public virtual HttpClient Client { get; init; } = new();
        public RestAPI(HttpClient client)
        {
            Client = client;


        }
        public async Task<object> SENDAsync(HttpMethod method, String URL, dynamic body, Dictionary<string, string> headers)
        {
            return await SENDAsync(method, URL, body, null, headers);
        }
        public async Task<object> SENDAsync(HttpMethod method, string URL, dynamic body, AuthenticationHeaderValue auth = null, Dictionary<string, string> headers = null)
        {
            DateTime start = DateTime.Now;
            HttpRequestMessage request = BuildRequestMessage(method, URL, body, auth, headers);

            HttpResponseMessage response = await Client.SendAsync(request);

            TimeSpan requestTime = DateTime.Now - start;
            timeSpans.Add(($"[{method}] {URL}", requestTime.TotalSeconds));

            return await ConsumeResponse(response);
        }

        public HttpRequestMessage BuildRequestMessage(HttpMethod method, string URL, dynamic body, AuthenticationHeaderValue auth, Dictionary<string, string> headers)
        {
            var request = new HttpRequestMessage();
            request.Method = method;
            if (auth != null)
            {
                request.Headers.Authorization = auth;
            }
            //if URL provided is a path,
            //enforce the base address' path prefix is part of the final uri
            //e.g.; 'api/quote/123' and '/quote/123/' should both work seamlessly
            //
            var uri = new Uri(URL, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
            {
                if (!uri.OriginalString.StartsWith(Client.BaseAddress.AbsolutePath, StringComparison.CurrentCultureIgnoreCase) &&
                    !('/' + uri.OriginalString).StartsWith(Client.BaseAddress.AbsolutePath, StringComparison.CurrentCultureIgnoreCase)
                    )
                {
                    uri = new Uri(Client.BaseAddress.AbsolutePath + (uri.OriginalString.StartsWith("/") ? uri.OriginalString : "/" + uri.OriginalString), UriKind.Relative);
                }
            }
            request.RequestUri = uri;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            if (body != null)
            {
                String bodyString = body is string ? body : ((JToken)JToken.FromObject(body)).ToString(Formatting.None);
                HttpContent content = new StringContent(bodyString, Encoding.UTF8, "application/json");
                request.Content = content;
            }

            return request;
        }

        public async Task<object> GETAsync(String URL, Dictionary<string, string> headers) => await GETAsync(URL, null, headers);


        public async Task<object> GETAsync(String URL, AuthenticationHeaderValue auth = null, Dictionary<string, string> headers = null)
            => await SENDAsync(HttpMethod.Get, URL, null, auth, headers);

        public async Task<object> POSTAsync(String URL, dynamic body, Dictionary<string, string> headers)
            => await POSTAsync(URL, body, null, headers);

        public async Task<object> POSTAsync(String URL, dynamic body, AuthenticationHeaderValue auth = null, Dictionary<string, string> headers = null)
            => await SENDAsync(HttpMethod.Post, URL, body, auth, headers);

        public async Task<object> POSTAsync(String URL, AuthenticationHeaderValue auth, HttpContent content)
        {

            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(URL);
            request.Content = content;
            request.Method = HttpMethod.Post;
            request.Headers.Authorization = auth;
            DateTime start = DateTime.Now;

            HttpResponseMessage response = await Client.SendAsync(request);

            TimeSpan requestTime = DateTime.Now - start;
            timeSpans.Add(($"[POST] {URL}", requestTime.TotalSeconds));

            return await ConsumeResponse(response);
        }


        public async Task<object> PATCHAsync(String URL, dynamic body, Dictionary<string, string> headers)
            => await PATCHAsync(URL, body, null, headers);


        public async Task<object> PATCHAsync(String URL, dynamic body, AuthenticationHeaderValue auth = null, Dictionary<string, string> headers = null)
            => await SENDAsync(HttpMethod.Patch, URL, body, auth, headers);


        public async Task<object> PUTAsync(string URL, object body, Dictionary<string, string> headers)
            => await PUTAsync(URL, body, null, headers);


        public async Task<object> PUTAsync(string URL, object body, AuthenticationHeaderValue auth = null, Dictionary<string, string> headers = null)
            => await SENDAsync(HttpMethod.Put, URL, body, auth, headers);

        private async Task<object> ConsumeResponse(HttpResponseMessage response)
        {
            var URL = response.RequestMessage.RequestUri.ToString();
            Log.Info($"[{response.RequestMessage.Method}].{URL}");
            string errorMsg = null;
            if (!response.IsSuccessStatusCode)
            {
                if (response.RequestMessage?.Content != null)
                {
                    Log.Critical(await response.RequestMessage?.Content?.ReadAsStringAsync() ?? "");
                }
                errorMsg = await response.Content?.ReadAsStringAsync();
                Log.Critical(errorMsg);
            }
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception(errorMsg ?? ex.Message, ex);
            }
            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.TryGetValues("content-type", out var contentType) && contentType.Contains("application/pdf"))
            {
                var filename = response.Content.Headers.GetValues("content-disposition")
                                                        .ElementAt(0)
                                                        .Split(";")[1]
                                                        .Substring(10)
                                                        .Trim('"');
                using (var file = File.Create(filename))
                {
                    var contentStream = await response.Content.ReadAsStreamAsync(); // get the actual content stream
                    contentStream.CopyTo(file); // copy that stream to the file stream

                    Log.Debug($"file for API request [/{URL}] \n location: " + file.Name);
                    response.Dispose();
                    return file.Name;
                }
            }
            else if (contentType != null && contentType.Any(it => it.Contains("json")))
            {
                var responseStr = response.Content.ReadAsStringAsync().Result;

                response.Dispose();

                try
                {
                    return JsonConvert.DeserializeObject<dynamic>(responseStr);
                }
                catch (JsonReaderException)
                {
                    return responseStr;
                }
            }
            else
            {
                return response.Content.ReadAsStringAsync().Result;
            }


        }
    }
}
