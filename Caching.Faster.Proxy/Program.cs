using System;
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
                    webBuilder
                        .UseStartup<Startup>()
                        .ConfigureKestrel(options =>
                        {
                            options.ConfigureEndpointDefaults(listenOptions =>
                            {
                                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                            });
                        });
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<K8SServiceDiscoveryHostedService>();
                });
    }
}
