using DnsClient.Internal;
using HitachiQA.Backend.API.Models;
using HitachiQA.Backend.API.Services;
using HitachiQA.Hooks.Browsers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HitachiQA.Backend.API.Test
{
    [TestClass]
    public sealed class RunMock
    {
        private readonly TestRunner _testRunner;
        public readonly TestContext _testContext;

        public RunMock(TestContext testContext)
        {
           _testRunner = LoadPlaywrightAndRunner();
           _testContext = testContext;
        }

      
        [TestMethod]
        public async Task TestHitachiUSWebsite()
        {
            string filePath = Path.Combine("MockData", "hitachius.json");

            var testRun = await LoadTestRun(filePath);
            await _testRunner.RunAsync(testRun);

        }

        [TestMethod]
        public async Task TestYahooSignup()
        {
            string filePath = Path.Combine("MockData", "yahoo_signup.json");

            var testRun = await LoadTestRun(filePath);
            var run = await _testRunner.RunAsync(testRun);

            foreach (var sc in run.Scenarios)
            {
                if(sc.VideoFilePath !=null)
                {
                    _testContext.AddResultFile(sc.VideoFilePath);
                }
            }

        }

        private static TestRunner LoadPlaywrightAndRunner()
        {
            var loggerFactory = LoggerFactory.Create(b =>
            {
                b.AddConsole();
            });
            var configBuilder = new ConfigurationBuilder();
            var settings = new
            {
                INSTALL_PLAYWRIGHT = "true"
            };
            var settingsDict = settings.GetType()
              .GetProperties()
              .ToDictionary(prop => prop.Name, prop => prop.GetValue(settings)?.ToString());
            configBuilder.AddInMemoryCollection(settingsDict);
            var config = configBuilder.Build();
            var playwrightHook = new PlaywrightHook(config, new BrowserIndicator { IsBrowserFeature = true });
            var testRunner = new TestRunner(playwrightHook, loggerFactory.CreateLogger<TestRunner>());
            return testRunner;
        }


        private static async Task<TestRun> LoadTestRun(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"JSON file not found: {filePath}");
            }

            string json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

            return JsonSerializer.Deserialize<TestRun>(json, options)!;
        }
    }
}
