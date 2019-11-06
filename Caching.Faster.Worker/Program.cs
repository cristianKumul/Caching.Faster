using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Caching.Faster.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rnd = new Random();
            var port = rnd.Next(100, 500);
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"https://*:9191");
            ThreadPool.SetMinThreads(6048, 6048);

            CreateHostBuilder(args).Build().Run();



        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            //.ConfigureLogging((c,a) => a.ClearProviders())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
