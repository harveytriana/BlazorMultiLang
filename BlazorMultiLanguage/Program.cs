// --------------------------------
// blazorspread.net
// --------------------------------
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorMultiLanguage
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            
            builder.Services.AddSingleton<LangService>();

            // setup application language
            var host = builder.Build();
            var langService = host.Services.GetService<LangService>();
            await langService.LoadLanguageAsync();

            await builder.Build().RunAsync();
        }
    }
}
