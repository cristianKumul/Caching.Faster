using Caching.Faster.Proxy.Hashing;
using Caching.Faster.Proxy.ServiceDiscovery.GKE;
using Caching.Faster.Proxy.ServiceDiscovery.GKE.HostedServices;
using BestDay.Prometheus.AspNetCore.Extensions.Implementations;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Prometheus;
using BestDay.Prometheus.AspNetCore.Extensions.Tracking;

namespace Caching.Faster.Proxy
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddGrpc( opt => {
                opt.CompressionProviders.Clear();
                opt.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.NoCompression;
                opt.MaxReceiveMessageSize = int.MaxValue;
                opt.MaxSendMessageSize = int.MaxValue;
                
            });
            services.AddSingleton<K8SServiceDiscovery>();
            services.AddSingleton<ConsistentHash>();
            services.AddSingleton<ChannelDistribution>();
            services.AddHostedService<K8SServiceDiscoveryHostedService>();
            services.AddTransient<CachingService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation("Starting up Proxy server {MachineName}", Environment.MachineName);
            var server = new Server
            {
                Ports = { new ServerPort("0.0.0.0", 90, ServerCredentials.Insecure) }
            };

            server.Services.Add(Caching.Faster.Proxy.ProxyCache.BindService(app.ApplicationServices.CreateScope().ServiceProvider.GetService<CachingService>()));

            server.Start();

            app.UseMetricServer();

            app.UseGrpcMiddlewares();

            app.GetGrpcPipelineBuilder()
            .UseExceptionHandler((context, ex) =>
            {
                logger.LogError(ex, "Error grpc service method: {Method} message: {Message}", context.Method, ex.Message);
            });
        }
    }
}
