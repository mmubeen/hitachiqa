using BoDi;
using HitachiQA.Helpers;
using HitachiQA.Hooks.Browsers;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace HitachiQA.Source.Hooks.HttpClientExtras
{
    public class AuthorizationClient
    {
        public HttpClient HttpClient { get; init; }
        public string? TenantId { get; init; }
        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; }
        public string? Username { get; init; }
        public string? Password { get; init; }
        private AuthorizationResponseResource? _latestAuthResponse { get; set; }
        private BrowserIndicator BrowserIndicator { get; init; }
        private IObjectContainer ObjectContainer { get; init; }
        private object lockGetBearerToken = new object();

        public AuthorizationClient(IConfiguration config, BrowserIndicator browserIndicator, IObjectContainer objectContainer)
        {
            BrowserIndicator = browserIndicator;
            ObjectContainer = objectContainer;

            HttpClient = new HttpClient();
            HttpClient.BaseAddress = new Uri("https://login.microsoftonline.com/");
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.Timeout = TimeSpan.FromSeconds(120);

            TenantId = config.GetVariable("API_TENANT_ID", true);
            ClientId = config.GetVariable("API_CLIENT_ID", true);
            ClientSecret = config.GetVariable("API_CLIENT_SECRET", true);
            Username = config.GetVariable("API_USERNAME", true);
            Password = config.GetVariable("API_PASSWORD", true);


        }
        public async Task<string> GetBearerTokenAsync()
        {
            if(BrowserIndicator.IsBrowserFeature)
            {
                var executor = ObjectContainer.Resolve<JSExecutor>();
                String currentUser = (string)executor.execute("return window.localStorage.getItem('currentUser')");
                var token = (string?)JsonConvert.DeserializeObject<dynamic>(currentUser)?["accessToken"];
                if(token!= null)
                    return token;
            }

            TenantId.NullGuard("please provide environment varialbe: API_TENANT_ID");
            ClientId.NullGuard("please provide environment varialbe: API_CLIENT_ID");
            ClientSecret.NullGuard("please provide environment varialbe: API_CLIENT_SECRET");
            Username.NullGuard("please provide environment varialbe: API_USERNAME");
            Password.NullGuard("please provide environment varialbe: API_PASSWORD");

            bool newToken;
            bool refreshToken;
            AuthorizationResponseResource result;
            lock (lockGetBearerToken)
            {
                newToken = _latestAuthResponse == null;
                refreshToken = !newToken && DateTime.Now.AddSeconds(_latestAuthResponse.ExpiresIn) <= DateTime.Now.AddSeconds(-5);
                result = _latestAuthResponse;
            }

            if (newToken)
                result = await GenerateNewTokenAsync();
            else if (refreshToken)
                result = await RefreshToken(result);

            if (newToken || refreshToken)
            {
                lock (lockGetBearerToken)
                {
                    _latestAuthResponse = result;
                }
            }

            return result?.AccessToken ??throw new NullReferenceException("result was null");

        }
        private async Task<AuthorizationResponseResource> GenerateNewTokenAsync()
        {
            Log.Debug("Generating new auth token");
            var reqContent = GetLoginHttpContent();

            var res = await HttpClient.PostAsync($"{TenantId}/oauth2/v2.0/token", reqContent);
            var auth = await res.Content.ReadFromJsonAsync<AuthorizationResponseResource>();
            return auth;
        }

        private async Task<AuthorizationResponseResource> RefreshToken(AuthorizationResponseResource previousAuthRes)
        {
            Log.Debug("Refreshing auth token");
            var reqContent = GetRefreshTokenHttpContent(previousAuthRes);

            var res = await HttpClient.PostAsync($"{TenantId}/oauth2/v2.0/token", reqContent);
            var auth = await res.Content.ReadFromJsonAsync<AuthorizationResponseResource>();
            return auth;
        }
        private HttpContent GetRefreshTokenHttpContent(AuthorizationResponseResource previousAuthRes)
        {
            return new FormUrlEncodedContent(new[]
               {
                new KeyValuePair<string,string>("grant_type",       "refresh_token" ),
                new KeyValuePair<string,string>("client_id",        ClientId ),
                new KeyValuePair<string,string>("client_secret",    ClientSecret ),
                new KeyValuePair<string,string>("scope",            $"openid {ClientId}/.default" ),
                new KeyValuePair<string,string>("refresh_token",    previousAuthRes.RefreshToken ),
            });
        }
        private HttpContent GetLoginHttpContent()
        {
            return new FormUrlEncodedContent(new[]
               {
                new KeyValuePair<string,string>("grant_type",       "PASSWORD" ),
                new KeyValuePair<string,string>("client_id",        ClientId ),
                new KeyValuePair<string,string>("client_secret",    ClientSecret ),
                new KeyValuePair<string,string>("scope",            $"openid {ClientId}/.default" ),
                new KeyValuePair<string,string>("userName",         Username ),
                new KeyValuePair<string,string>("Password",         Password )
            });
        }

        public class AuthorizationResponseResource
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("scope")]
            public string Scope { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("id_token")]
            public string IdToken { get; set; }

            [JsonPropertyName("error")]
            public string Error { get; set; }

            [JsonPropertyName("error_description")]
            public string ErrorDescription { get; set; }

            [JsonPropertyName("error_codes")]
            public List<int> ErrorCodes { get; set; }

            [JsonPropertyName("timestamp")]
            public string Timestamp { get; set; }

            [JsonPropertyName("trace_id")]
            public string TraceId { get; set; }

            [JsonPropertyName("correlation_id")]
            public string CorrelationId { get; set; }
        }

    }

}
