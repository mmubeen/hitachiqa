using Azure.Core;
using FluentAssertions.Common;
using HitachiQA.Helpers;
using HitachiQA.HttpClients.Authorization;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HitachiQA.HttpClients
{
    public class AuthorizationClient : TokenCredential
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private InteractiveAuthBase _interactiveAuth = null;
        private readonly InteractivePlaywrightAuth _playwrightInteractive;
        private readonly InteractiveWebdriverAuth _webDriverInteractive;

        public Credentials CurrentCredentials { get; set; }
        public HttpClient HttpClient { get; init; }
        public IConfiguration Config { get; init; }
        public string TenantId { get; init; }
        public string ClientId { get; init; }
        public string ClientSecret { get; init; }
        public string Username { get; init; }
        public string Password { get; init; }



        public AuthorizationClient(InteractivePlaywrightAuth playwrightInteractive, InteractiveWebdriverAuth webDriverInteractive, IConfiguration config)
        {
            HttpClient = new HttpClient();
            HttpClient.BaseAddress = new Uri("https://login.microsoftonline.com/");
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.Timeout = TimeSpan.FromSeconds(120);
            Config = config;
            TenantId = config.GetVariable("API_TENANT_ID", true);
            ClientId = config.GetVariable("API_CLIENT_ID", true);
            ClientSecret = config.GetVariable("API_CLIENT_SECRET", true);
            Username = config.GetVariable("API_USERNAME", true);
            Password = config.GetVariable("API_PASSWORD", true);
            _playwrightInteractive = playwrightInteractive;
            _webDriverInteractive = webDriverInteractive;
        }

        public async Task<string> GetBearerTokenAsync(bool forceNewToken = false, bool? useInteractiveAuth = null)
        {
            await _semaphore.WaitAsync();

            try
            {
                useInteractiveAuth ??= GetIsInteractiveAuthFromConfig();

                //use browser for authentication (local dev)
                if (useInteractiveAuth ?? false)
                {
                    if (_interactiveAuth == null)
                    {
                        var framework = Config.GetVariable("FRAMEWORK", true);
                        if (framework != null && framework.Equals("playwright", StringComparison.CurrentCultureIgnoreCase))
                        {
                            _interactiveAuth = _playwrightInteractive;
                        }
                        else
                        {
                            _interactiveAuth = _webDriverInteractive;
                        }
                    }

                    var filePath = GetFilePath();
                    //use local cache to reuse token
                    if (CurrentCredentials == null && File.Exists(filePath))
                    {
                        var content = await File.ReadAllTextAsync(filePath);
                        CurrentCredentials = JsonSerializer.Deserialize<Credentials>(content);
                    }
                    //generate new token if needed
                    if (forceNewToken || CurrentCredentials == null || CurrentCredentials.ExpiryDateTime <= DateTime.Now)
                    {
                        CurrentCredentials = await _interactiveAuth.AuthenticateUsingBrowserAsync();
                        SaveLatestCredsLocally();
                    }

                }
                //use service account (pipeline execution)
                else
                {
                    //acquire new token
                    if (forceNewToken || CurrentCredentials == null || CurrentCredentials.ExpiryDateTime <= DateTime.Now)
                    {
                        var authRes = await AutheniticateUsingClientSecretAsync();
                        CurrentCredentials = authRes.ToCredentials();
                    }

                }



                return CurrentCredentials.AccessToken;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private bool GetIsInteractiveAuthFromConfig()
        {
            var iaStr = Config.GetVariable("ENABLE_INTERACTIVE_AUTH", true);

            //use browser for authentication (made for local dev)
            if (bool.TryParse(iaStr, out var useInteractiveAuth))
                return useInteractiveAuth;

            return false;
        }

        private void SaveLatestCredsLocally()
        {
            var obj = CurrentCredentials.ToJObject();
            var filePath = GetFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new NullReferenceException($"[SaveLatestCredsLocally] {filePath} directory returned null"));
            File.WriteAllText(filePath, obj.ToString());
        }
        private string GetFilePath() => $"./localcache/token_{Config.GetVariable("HOST").Replace("https:", "").Replace("http:", "").Replace("/", "")}.json";

        private async Task<AuthorizationResponseResource> AutheniticateUsingClientSecretAsync()
        {
            ValidateProperties();
            var reqContent = GetLoginHttpContent();

            var res = await HttpClient.PostAsync($"{TenantId}/oauth2/v2.0/token", reqContent);
            var auth = await res.Content.ReadFromJsonAsync<AuthorizationResponseResource>();
            auth.ExpiryDateTime = DateTime.Now.AddSeconds(auth.ExpiresIn - 30);
            return auth;
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

        private void ValidateProperties()
        {
            var missingProperties = new List<string>();

            if (string.IsNullOrWhiteSpace(TenantId)) missingProperties.Add("API_TENANT_ID");
            if (string.IsNullOrWhiteSpace(ClientId)) missingProperties.Add("API_CLIENT_ID");
            if (string.IsNullOrWhiteSpace(ClientSecret)) missingProperties.Add("API_CLIENT_SECRET");
            if (string.IsNullOrWhiteSpace(Username)) missingProperties.Add("API_USERNAME");
            if (string.IsNullOrWhiteSpace(Password)) missingProperties.Add("API_PASSWORD");

            if (missingProperties.Count != 0)
            {
                throw new Exception($"The following configuration variables are missing:\n {string.Join(", ", missingProperties)}");
            }
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return GetTokenAsync(requestContext, cancellationToken).Result;
        }

        public async override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var token = await GetBearerTokenAsync();
            var at = new AccessToken(token, CurrentCredentials.ExpiryDateTime.Value.ToDateTimeOffset());
            return at;

        }
    }
}
