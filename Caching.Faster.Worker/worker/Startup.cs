using BestDay.Prometheus.AspNetCore.Extensions.Implementations;
using BestDay.Prometheus.AspNetCore.Extensions.Tracking;
using Caching.Faster.Worker.Collectors;
using Caching.Faster.Worker.Services;
using Caching.Faster.Workers.Extensions;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Caching.Faster.Worker
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddFaster();
            services.AddHostedService<EvictionHostedService>();
            services.AddSingleton<EvictedMetric>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseFasterWithGrpc();

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
