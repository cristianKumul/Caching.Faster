using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caching.Faster.Proxy.ServiceDiscovery.GKE.HostedServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace Caching.Faster.Proxy
{
    public class Program
    {
        private static bool EnableLogging => bool.Parse(Environment.GetEnvironmentVariable("EnableLogging") ?? "false");
        public static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "https://*:88");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((c, a) =>
                {
                    if (!EnableLogging)
                        a.ClearProviders();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<K8SServiceDiscoveryHostedService>();
                });
    }
}
