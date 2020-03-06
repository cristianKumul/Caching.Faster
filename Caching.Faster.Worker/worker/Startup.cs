using Caching.Faster.Worker.Collectors;
using Caching.Faster.Worker.Services;
using Caching.Faster.Workers.Extensions;
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
            app.UseFasterWithGrpc(logger);

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapMetrics());
        }
    }
}
