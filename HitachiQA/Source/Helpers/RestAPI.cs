using System;
using System.Text;
using OpenQA.Selenium;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using HitachiQA.Driver;
using BoDi;
using Microsoft.Extensions.Configuration;
using HitachiQA.Hooks;

namespace HitachiQA.Helpers
{
    public class RestAPI
    {
        private readonly BrowserIndicator BrowserIndicator;
        private readonly JSExecutor JSExecutor;
        private readonly IConfiguration Configuration;
        public RestAPI(ObjectContainer objectContainer)
        {
            this.BrowserIndicator = objectContainer.Resolve<BrowserIndicator>();
            if(!BrowserIndicator.isNoBrowserFeature)
            {
                JSExecutor = objectContainer.Resolve<JSExecutor>();
            }
            this.Configuration = objectContainer.Resolve<IConfiguration>();
        }
        public List<(string key, double seconds)> timeSpans = new List<(string key, double seconds)>();
        public dynamic GET( String URL)
        {
            return GET(URL, new AuthenticationHeaderValue("Bearer", getAuthToken()));
        }
        public dynamic GET(String URL, AuthenticationHeaderValue Authorization, Dictionary<string, string> headers = null)
        {

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = Authorization;
            client.DefaultRequestHeaders.Add("User-Agent", "Apollo Automation");

            if (headers != null)
            {
                foreach(var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            DateTime start = DateTime.Now;
            HttpResponseMessage response = client.GetAsync(processURL(URL)).Result;
            TimeSpan requestTime = DateTime.Now - start;

            timeSpans.Add(($"[GET] {URL}", requestTime.TotalSeconds));

            client.Dispose();
            return ConsumeResponse(response, URL);
        }
        public dynamic POST(String URL, dynamic body)
        {
            return POST(URL, body, new AuthenticationHeaderValue("Bearer", getAuthToken()));
        }

        public dynamic POST(String URL, dynamic body, AuthenticationHeaderValue Authorization)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(processURL(URL));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = Authorization;
            client.Timeout = TimeSpan.FromSeconds(600);

            String bodyString;
            if (body is String)
            {
                bodyString = body;
            }
            else
            {
                bodyString = JsonConvert.SerializeObject(body);
            }

            HttpContent content = new StringContent(bodyString, Encoding.UTF8, "application/json");

            DateTime start = DateTime.Now;

            HttpResponseMessage response = client.PostAsync(processURL(URL), content).Result;

            TimeSpan requestTime = DateTime.Now - start;
            timeSpans.Add(($"[POST] {URL}", requestTime.TotalSeconds));


            client.Dispose();
            return ConsumeResponse(response, URL, body);
        }
        public dynamic POST(String URL, AuthenticationHeaderValue Authorization, HttpContent content)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(processURL(URL));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = Authorization;
            client.Timeout = TimeSpan.FromSeconds(280);

            DateTime start = DateTime.Now;

            HttpResponseMessage response = client.PostAsync(processURL(URL), content).Result;

            TimeSpan requestTime = DateTime.Now - start;
            timeSpans.Add(($"[POST] {URL}", requestTime.TotalSeconds));

            client.Dispose();
            return ConsumeResponse(response, URL, content);
        }


        public dynamic PATCH(String URL, dynamic body)
        {
            return PATCH(URL, body, new AuthenticationHeaderValue("Bearer", getAuthToken()));
        }

        public dynamic PATCH(String URL, dynamic body, AuthenticationHeaderValue Authorization)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(processURL(URL));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = Authorization;

            String bodyString;
            if (body is String)
            {
                bodyString = body;
            }
            else
            {
                bodyString = JsonConvert.SerializeObject(body);
            }

            HttpContent content = new StringContent(bodyString, Encoding.UTF8, "application/json");

            DateTime start = DateTime.Now;

            HttpResponseMessage response = client.PatchAsync(processURL(URL), content).Result;

            TimeSpan requestTime = DateTime.Now - start;
            timeSpans.Add(($"[PATCH] {URL}", requestTime.TotalSeconds));

            client.Dispose();
            return ConsumeResponse(response, URL,body);
        }

        public dynamic PUT(string URL, dynamic body)
        {
            return PUT(URL, body, new AuthenticationHeaderValue("Bearer", getAuthToken()));
        }

        public dynamic PUT(string URL, dynamic body, AuthenticationHeaderValue Authorization)
        {
            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri(processURL(URL))
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = Authorization;

            string bodyString;
            if (body is string)
            {
                bodyString = body;
            }
            else
            {
                bodyString = JsonConvert.SerializeObject(body);
            }

            HttpContent content = new StringContent(bodyString, Encoding.UTF8, "application/json");

            DateTime start = DateTime.Now;

            HttpResponseMessage response = client.PutAsync(processURL(URL), content).Result;

            TimeSpan requestTime = DateTime.Now - start;
            timeSpans.Add(($"[PUT] {URL}", requestTime.TotalSeconds));

            client.Dispose();
            return ConsumeResponse(response, URL, body);
        }

        private String processURL(String URL)
        {
            if (!URL.StartsWith("http"))
            {
                if (!URL.StartsWith("/"))
                {
                    return this.Configuration.GetVariable("SERVER_HOST") + "/"+URL;
                }
                return this.Configuration.GetVariable("SERVER_HOST") + URL;
            }
            return URL;
        }

        private dynamic ConsumeResponse(HttpResponseMessage response, string URL, dynamic body=null)
        {
            if(!response.IsSuccessStatusCode)
            {
                Log.Critical(processURL(URL));
                Log.Critical(body);
                Log.Critical(response.Content.ReadAsStringAsync().Result);
            }
            response.EnsureSuccessStatusCode();
            String dataObjects = response.Content.ReadAsStringAsync().Result;
            response.Dispose();

            try
            {
                return JsonConvert.DeserializeObject<dynamic>(dataObjects);
            }catch(JsonReaderException)
            {
                return dataObjects;
            }
        }

        private String getAuthToken()
        {
            if(BrowserIndicator.isNoBrowserFeature)
            {
                return getAuthTokenAPI();
            }
            String currentUser= (string)JSExecutor.execute("return window.localStorage.getItem('currentUser')");
            return (String)JsonConvert.DeserializeObject<dynamic>(currentUser)["accessToken"];
        }

        private String _APIToken = null;

        private String getAuthTokenAPI()
        {
            if (_APIToken == null)
            {
                string TenantId = Configuration.GetVariable("API_TENANT_ID");
                string ClientId = Configuration.GetVariable("API_CLIENT_ID");
                string ClientSecret = Configuration.GetVariable("API_CLIENT_SECRET");
                string Username = Configuration.GetVariable("API_USERNAME");
                string Password = Configuration.GetVariable("API_PASSWORD");

                HttpContent content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string,string>("grant_type",       "PASSWORD" ),
                new KeyValuePair<string,string>("client_id",        ClientId ),
                new KeyValuePair<string,string>("client_secret",    ClientSecret ),
                new KeyValuePair<string,string>("scope",            $"openid {ClientId}/.default" ),
                new KeyValuePair<string,string>("userName",         Username ),
                new KeyValuePair<string,string>("Password",         Password )
            });

                var response = POST($"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token", null, content);

                _APIToken = response["access_token"];
            }
            return _APIToken;
        }
    }
}
