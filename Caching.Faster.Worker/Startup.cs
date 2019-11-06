using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caching.Faster.Workers.Core;
using FASTER.core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Caching.Faster.Worker
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

            var log = Devices.CreateLogDevice(Path.GetTempPath() + "hlog.log", deleteOnClose: true);
            var objlog = Devices.CreateLogDevice(Path.GetTempPath() + "hlog.obj.log", deleteOnClose: true);


            var logSettings = new LogSettings
            {
                LogDevice = log,
                ObjectLogDevice = objlog
            };
            
            //cached pages
            //logSettings.ReadCacheSettings = new ReadCacheSettings();
            //logSettings.ReadCacheSettings.PageSizeBits = 1024 * 1024 * 10;
            //logSettings.ReadCacheSettings.MemorySizeBits = 1024 * 1024 * 10;
            

            services.AddGrpc();
            services.AddSingleton(new FasterKV<Key, Value, Input, Output, CacheContext, CacheFunctions>(
                    1L << 20,
                    new CacheFunctions(),
                    logSettings,
                    null,
                    new SerializerSettings<Key, Value> { keySerializer = () => new CacheKeySerializer(), valueSerializer = () => new CacheValueSerializer() }
                ));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<CachingService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
