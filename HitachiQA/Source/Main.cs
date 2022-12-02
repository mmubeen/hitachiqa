using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using TechTalk.SpecFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BoDi;
using Microsoft.Extensions.Azure;
using HitachiQA.Helpers;
using System.Runtime.CompilerServices;

namespace HitachiQA
{
    [Binding]
    public static class Main
    {
        public static readonly IConfiguration Configuration = BuildConfig();
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static ObjectContainer ObjectContainer;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [BeforeScenario(Order =1)]
        public static void LoadConfig(ObjectContainer oc)
        {
            oc.RegisterInstanceAs<IConfiguration>(Configuration);
        }

        [BeforeScenario(Order = 1)]
        public static void LoadObjectContainer(ObjectContainer oc)
        {
            ObjectContainer = oc;
        }

        public static string? GetVariable(this IConfiguration config, string VariableName, bool optional)
        {
            var VarName = config.GetChildren().FirstOrDefault(it => it.Key == VariableName + "_VARNAME")?.Value;
            if (IsValid(VarName))
            {
                VarName.NullGuard();
                return config.GetVariable(VarName);
            }
            if(optional)
            {
                var val = config.GetChildren().FirstOrDefault(it => it.Key == VariableName)?.Value;
                return val;
            }

            var value = config.GetChildren().FirstOrDefault(it => it.Key == VariableName)?.Value;
            return value ?? throw new ArgumentNullException($"Variable: {VariableName} was not found");
        }
        public static string GetVariable(this IConfiguration config, string VariableName)
        {
            return config.GetVariable(VariableName, false) ?? throw new NullReferenceException();
        }

        public static IConfiguration BuildConfig()
        {
            Console.WriteLine("BUILDING CONFIG");
            var builder = new ConfigurationBuilder()
                           .SetBasePath(BasePath)
                           .AddJsonFile("appsettings.json")
                           .AddEnvironmentVariables()
                           .AddUserSecrets(ExecutingAssembly);
                           

            var config = builder.Build();


            var appConfigUri = config.GetVariable("APP_CONFIG_URI", true);
            var keyVaultUri = config.GetVariable("KEYVAULT_URI", true);

            var AUTappConfigUri = config.GetVariable("AUT_APP_CONFIG_URI", true);
            var AUTkeyVaultUri = config.GetVariable("AUT_KEYVAULT_URI", true);
 
            attemptLoadAppConfig(builder, appConfigUri, "App Config");
            attemptLoadKeyVault(builder, keyVaultUri, "Keyvalut");

            attemptLoadAppConfig(builder, AUTappConfigUri, "AUT App Config");
            attemptLoadKeyVault(builder, AUTkeyVaultUri, "AUT Keyvault");

            config = builder.Build();
            Console.WriteLine("BUILT CONFIG SUCCESSFULLY");

            CheckPossibleVariableTypos(config);

            return config;

        }

        private static void attemptLoadKeyVault(IConfigurationBuilder builder, string? keyVaultUri, string displayName)
        {
            if (IsValid(keyVaultUri))
            {
                Console.WriteLine($"LOADING {displayName}: {keyVaultUri}");

                keyVaultUri.NullGuard();
                builder.AddAzureKeyVault(new Uri(keyVaultUri),
                new DefaultAzureCredential()
                );
                Console.WriteLine($"LOADED {displayName} SUCCESSFULLY: {keyVaultUri}");

            }
            else
            {
                Console.WriteLine($"No {displayName} Loaded");
            }
        }
        private static void attemptLoadAppConfig(IConfigurationBuilder builder, string? appConfigUri, string displayName)
        {
            if (IsValid(appConfigUri)){
                Console.WriteLine($"LOADING {displayName}: {appConfigUri}");
                appConfigUri.NullGuard();
                builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(appConfigUri), new DefaultAzureCredential());
                });
                Console.WriteLine($"LOADED {displayName} SUCCESSFULLY: {appConfigUri}");
            }
            else{
                Console.WriteLine($"No {displayName} Loaded");
            }
        }


        private static Assembly ExecutingAssembly => Assembly.GetExecutingAssembly();
        private static string BasePath
        {
            get {
                var path = Assembly.GetExecutingAssembly().Location;
                var uri = new Uri(path);
                var lastSegment = uri.Segments.Last();

                return path.Substring(0, path.IndexOf(lastSegment));
            }
        }
        public static bool IsValid(string? uri) => uri != null && uri.Length > 0 && uri.ToUpper() != "TBD";

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
            "AUT_KEYVAULT_URI",
            "COSMOS_URI",
            "COSMOS_API_KEY",
            "COSMOS_DATABASE_NAME",
            "SQL_CONNECTION_STRING"
        };

        public static void CheckPossibleVariableTypos(IConfiguration config)
        {
            foreach(var item in config.GetChildren())
            {
                if (item.Key == null) continue;
                if (KnownVariables.Contains(item.Key)) continue;

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
