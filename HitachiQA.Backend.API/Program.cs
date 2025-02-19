using HitachiQA.Backend.API.Interfaces;
using HitachiQA.Backend.API.Services;
using HitachiQA.Hooks.Browsers;
using HitachiQA.Playwright;

namespace HitachiQA.Backend.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                })
                ;
            builder.Services.AddSingleton(p =>
            {
                var i = new BrowserIndicator();
                i.IsBrowserFeature = true;
                return i;
            });
            builder.Services.AddTransient<PlaywrightHook>();
            builder.Services.AddTransient<ITestRunner, TestRunner>();

            var app = builder.Build();

            app.UseRouting();
            app.MapControllers();
            app.MapGet("/", () => "Hello World!");

            PlaywrightHook.InstallPlaywright();
            app.Run();
        }
    }
}
