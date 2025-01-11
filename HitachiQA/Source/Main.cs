using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using HitachiQA.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Reqnroll.BoDi;
using System.Reflection;

namespace HitachiQA
{
    [Binding]
    public static class Main
    {
        private static IConfigurationRoot _config;
        public static IConfiguration Configuration { get { return _config ??= BuildConfig(); } }
        public static IObjectContainer ObjectContainer;
        public static string RunId { get; set; }
        public static LogLevel LogLevel => Enum.Parse<LogLevel>(Configuration["Logging:LogLevel:Default"]);
        public static Assembly ExecutingAssembly { get; set; }

        private static object _configLock = new();

        [BeforeFeature(Order = 1)]
        public static void LoadConfig(IObjectContainer oc)
        {
            oc.RegisterInstanceAs<IConfiguration>(Configuration);
        }

        [BeforeScenario(Order = 1)]
        public static void LoadObjectContainer(IObjectContainer oc)
        {
            ObjectContainer = oc;
        }

        public static void SetVariable(this IConfiguration config, string key, string value)
        {
            lock (_configLock)
            {
                config[key] = value; // Perform the set operation
            }

        }

        public static string GetVariable(this IConfiguration config, string variableName, bool optional)
        {
            lock (_configLock)
            {

                try
                {
                    var varName = config.GetChildren().FirstOrDefault(it => it.Key == variableName + "_VARNAME")?.Value;
                    if (IsValid(varName))
                    {
                        varName.NullGuard();
                        try
                        {
                            return config.GetVariable(varName, optional);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error retireving variable {varName}", ex);
                        }
                    }
                    if (optional)
                    {
                        var val = config.GetValue<string>(variableName);
                        return val;
                    }

                    return config.GetValue<string>(variableName) ?? throw new ArgumentNullException($"Variable: {variableName} was not found");

                }
                catch (Exception)
                {
                    DumpDebugView(_config);
                    throw;

                }
            }
        }
        private static void DumpDebugView(IConfigurationRoot config)
        {
            try
            {
                var logFileSemaphore = new Semaphore(1, 1, "Global\\LogFileSemaphore");

                var debugRaw = config.GetDebugView(v =>
                {
                    if (v.Value == null)
                        return "null";
                    if (v.Value == "")
                        return "<EmptyStringVal>";
                    if (string.IsNullOrWhiteSpace(v.Value))
                        return "<WhiteSpaceVal>";
                    return "<SecretValue>";

                });

                logFileSemaphore.WaitOne(3000);
                try
                {
                    if (config != null)
                    {
                        Directory.CreateDirectory("./logs");
                        var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        var filePath = Path.GetFullPath($"./logs/config_debug_view_{timestamp}.txt");
                        File.WriteAllText(filePath, debugRaw);
                    }

                }
                finally
                {
                    logFileSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing debug view to file: {ex.Message}");
            }

        }
        public static string GetVariable(this IConfiguration config, string variableName)
        {
            return config.GetVariable(variableName, false) ?? throw new NullReferenceException();
        }

        public static IConfigurationRoot BuildConfig()
        {


            Console.WriteLine("BUILDING CONFIG");
            ExecutingAssembly = GetExecutingAssembly();
            var builder = new ConfigurationBuilder()
                           .SetBasePath(BasePath)
                           .AddJsonFile("appsettings.json", true)
                           .AddEnvironmentVariables()
                           .AddUserSecrets(ExecutingAssembly);
            Console.WriteLine("Executing Assembly:" + ExecutingAssembly.FullName);
            var config = builder.Build();
            var configSourceType = config.GetVariable("CONFIG_SOURCE_TYPE", true);
            var configSource = config.GetVariable("CONFIG_SOURCE", true);

            builder = builder.LoadConfigurationSource(configSourceType, configSource);
            config = builder.Build();
            CheckPossibleVariableTypos(config);
            return config;
        }

        private static IConfigurationBuilder LoadConfigurationSource(this IConfigurationBuilder builder, string configSourceType, string configSource)
        {
            var config = builder.Build();
            var disableAuth = config.GetVariable("DISABLE_AZURE_AUTHENTICATION", true);

            if (bool.TryParse(disableAuth, out var disabled) && disabled)
            {
                return builder;
            }
            switch (configSourceType?.ToUpper())
            {

                case "KEYVAULT":
                    if (!IsValid(configSource))
                        throw new Exception("CONFIG_SOURCE keyvault URI is required when using Keyvault as a config source");
                    attemptLoadKeyVault(builder, configSource, "Keyvault Config Source", IsCachedSecrets(config));

                    //this keyvault is persisted across all environments
                    attemptLoadKeyVault(builder, config.GetVariable("KEYVAULT_URI", true), "Automaion Keyvault", IsCachedSecrets(config));
                    break;
                case "LOCALSETTINGS":
                    var localSettingsPath = configSource;
                    if (!File.Exists(localSettingsPath))
                        throw new Exception($"CONFIG_SOURCE file does not exist at {Path.GetFullPath(localSettingsPath)}");

                    var obj = JObject.Parse(File.ReadAllText(localSettingsPath));
                    builder.AddInMemoryCollection(obj.Value<JObject>("Values").ToObject<IDictionary<string, string>>());
                    builder.AddInMemoryCollection(obj.Value<JObject>("ConnectionStrings").ToObject<IDictionary<string, string>>());

                    //this keyvault is persisted across all environments
                    attemptLoadKeyVault(builder, config.GetVariable("KEYVAULT_URI", true), "Automaion Keyvault", IsCachedSecrets(config));
                    break;

                case "JSON":
                    var jsonFilePath = configSource;
                    if (!File.Exists(jsonFilePath))
                        throw new Exception($"CONFIG_SOURCE file does not exist at {Path.GetFullPath(jsonFilePath)}");

                    builder.AddJsonFile(jsonFilePath);

                    //this keyvault is persisted across all environments
                    attemptLoadKeyVault(builder, config.GetVariable("KEYVAULT_URI", true), "Automaion Keyvault", IsCachedSecrets(config));
                    break;

                case "NONE":
                    break;

                default:
                    if (File.Exists("local.settings.json"))
                    {
                        builder.LoadConfigurationSource("LOCALSETTINGS", "local.settings.json");
                    }
                    else if ("HitachiQA.UnitTests" == GetExecutingAssembly().GetName().Name)
                    {
                        //unit test - no config needed
                    }
                    else
                    {
                        throw new Exception("\nplease select a .runsetting file \n "
                        + new Uri("https://learn.microsoft.com/en-us/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file?view=vs-2022"));
                    }
                    break;
            }

            return builder;
        }

        private static bool IsCachedSecrets(IConfiguration config) => config.GetVariable("CACHE_SECRETS", true) is string secret_cache && secret_cache != null && secret_cache.ToLower() == "true";

        private static void attemptLoadKeyVault(IConfigurationBuilder builder, string keyVaultUri, string displayName, bool useCache = false)
        {

            if (IsValid(keyVaultUri))
            {
                var filePath = $"./localcache/secrets{keyVaultUri.Replace("https://", "").Replace("net/", "net")}.json";

                if (useCache)
                {
                    if (File.Exists(filePath))
                    {
                        builder.AddJsonFile(filePath);
                        return;
                    }
                }

                Console.WriteLine($"LOADING {displayName}: {keyVaultUri}");

                keyVaultUri.NullGuard();


                if (useCache)
                {
                    var newBuilder = new ConfigurationBuilder()
                        .AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());

                    var config = newBuilder.Build();
                    var secretsToCache = new List<string>();
                    var allKeys = config.GetChildren().Select(it => it.Key);

                    var secretProviders = config.Providers.Where(it => it is AzureKeyVaultConfigurationProvider).Select(it => (AzureKeyVaultConfigurationProvider)it);
                    foreach (var provider in secretProviders)
                    {
                        var providerKeys = allKeys.SelectMany(key =>
                        {
                            if (provider.TryGet(key, out var strValue) && strValue != null)
                                return new[] { key };

                            return provider.GetChildKeys(Array.Empty<string>(), key).Select(childKey => $"{key}:{childKey}");
                        }).ToList();

                        secretsToCache.AddRange(providerKeys);

                    }
                    var secrets = new JObject();
                    secretsToCache.ForEach(it => secrets.Add(it, config.GetVariable(it)));
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new NullReferenceException($"{filePath} directory returned null"));
                    File.WriteAllText(filePath, secrets.ToString());

                    //recursive call to load from cache
                    attemptLoadKeyVault(builder, keyVaultUri, displayName, useCache);

                }
                else
                {
                    builder.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
                }

                Console.WriteLine($"LOADED {displayName} SUCCESSFULLY: {keyVaultUri}");
            }
            else
            {
                Console.WriteLine($"No {displayName} Loaded");
            }
        }

        private static void attemptLoadAppConfig(IConfigurationBuilder builder, string appConfigUri, string displayName)
        {
            if (IsValid(appConfigUri))
            {
                Console.WriteLine($"LOADING {displayName}: {appConfigUri}");
                appConfigUri.NullGuard();
                builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(appConfigUri), new DefaultAzureCredential());
                });
                Console.WriteLine($"LOADED {displayName} SUCCESSFULLY: {appConfigUri}");
            }
            else
            {
                Console.WriteLine($"No {displayName} Loaded");
            }
        }

        private static Assembly GetExecutingAssembly()
        {
            // Get all loaded assemblies
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Identify the test assembly based on unique characteristics
            var testAssembly = loadedAssemblies
                .FirstOrDefault(assembly =>
                {
                    // Skip system and framework assemblies
                    var assemblyName = assembly.GetName()?.Name;
                    if (string.IsNullOrEmpty(assemblyName) ||
                        assemblyName.StartsWith("System") ||
                        assemblyName.StartsWith("Microsoft") ||
                        assemblyName == typeof(Main).Assembly.GetName()?.Name)
                    {
                        return false;
                    }

                    // Check for [TestClass] or similar attributes
                    return assembly.GetTypes().Any(type =>
                        type.GetCustomAttributes(false).Any(attr =>
                            attr.GetType().Name == nameof(TestClassAttribute)));
                });

            // Fallback: Return Trident's default assembly
            return testAssembly ?? typeof(Main).Assembly;
        }


        private static string BasePath
        {
            get
            {
                var path = Assembly.GetExecutingAssembly().Location;
                var uri = new Uri(path);
                var lastSegment = uri.Segments.Last();

                return path.Substring(0, path.IndexOf(lastSegment));
            }
        }
        public static bool IsValid(string uri) => uri != null && uri.Length > 0 && uri.ToUpper() != "TBD";

        /// <summary>
        /// On update, please update Readme list of variables and description
        /// </summary>
        public static string[] KnownVariables => new string[] {
            "HOST",
            "BROWSER",
            "SERVER_HOST",
            "API_TENANT_ID",
            "API_CLIENT_ID",
            "API_CLIENT_SECRET",
            "API_USERNAME",
            "API_PASSWORD",
            "KEYVAULT_URI",
            "APP_CONFIG_URI",
            "AUT_APP_CONFIG_URI",
            "CONFIG_SOURCE",
            "CONFIG_SOURCE_TYPE",
            "COSMOS_URI",
            "COSMOS_API_KEY",
            "COSMOS_DATABASE_NAME",
            "SQL_CONNECTION_STRING",
            "COSMOS_CONNECTION_STRING"
        };

        public static void CheckPossibleVariableTypos(IConfiguration config)
        {
            foreach (var item in config.GetChildren())
            {
                if (item.Key == null) continue;
                if (KnownVariables.Contains(item.Key.Replace("_VARNAME", ""))) continue;

                foreach (var knownVariable in KnownVariables)
                {
                    var percent = Functions.CalculateSimilarityPercent(knownVariable, item.Key);

                    if (percent > 0.70M)
                    {
                        Console.WriteLine($"WARNING! provided variable {item.Key}={item.Value} is similar to the following known vairable {knownVariable}. It's possible that it's a typo");
                    }

                }
            }
        }

    }


}
