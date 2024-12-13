using BoDi;
using HitachiQA.Helpers;
using HitachiQA.Hooks.Browsers;
using HitachiQA.Source.HttpClients.Authorization;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using Polly;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverManager;

namespace HitachiQA.Source.HttpClients
{
    public class AuthorizationClient
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private InteractiveAuthBase? _interactiveAuth = null;
        private readonly InteractivePlaywrightAuth _playwrightInteractive;
        private readonly InteractiveWebdriverAuth _webDriverInteractive;

        private Credentials _latestCreds { get; set; }


        public HttpClient HttpClient { get; init; }
        public IConfiguration Config { get; init; }
        public string? TenantId { get; init; }
        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; }
        public string? Username { get; init; }
        public string? Password { get; init; }



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
                if (useInteractiveAuth??false)
                {
                    if(_interactiveAuth == null)
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
                    if (_latestCreds == null && File.Exists(filePath))
                    {
                        var content = await File.ReadAllTextAsync(filePath);
                        _latestCreds = JsonSerializer.Deserialize<Credentials>(content);
                    }
                    //generate new token if needed
                    if (forceNewToken || _latestCreds == null || _latestCreds.ExpiryDateTime <= DateTime.Now)
                    {
                        _latestCreds = await _interactiveAuth.AuthenticateUsingBrowserAsync();
                        SaveLatestCredsLocally();
                    }

                }
                //use service account (pipeline execution)
                else
                {
                    //acquire new token
                    if (forceNewToken || _latestCreds == null || _latestCreds.ExpiryDateTime <= DateTime.Now)
                    {
                        var authRes = await AutheniticateUsingClientSecretAsync();
                        _latestCreds = authRes.ToCredentials();
                    }

                }



                return _latestCreds.AccessToken;
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
            if(bool.TryParse(iaStr, out var useInteractiveAuth))
                return useInteractiveAuth;
            
            return false;
        }

        private void SaveLatestCredsLocally()
        {
            var obj = _latestCreds.ToJObject();
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

            if (missingProperties.Any())
            {
                throw new Exception($"The following configuration variables are missing:\n {string.Join(", ", missingProperties)}");
            }
        }



    }
}
