using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caching.Faster.Workers.Core;
using FASTER.core;
using Grpc.Core;
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



            //cached pages
            //logSettings.ReadCacheSettings = new ReadCacheSettings();
            //logSettings.ReadCacheSettings.PageSizeBits = 1024 * 1024 * 10;
            //logSettings.ReadCacheSettings.MemorySizeBits = 1024 * 1024 * 10;


            services.AddGrpc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var server = new Server
            {
                Ports = { new ServerPort("0.0.0.0", 90, ServerCredentials.Insecure) }
            };

            var log = Devices.CreateLogDevice("log", deleteOnClose: true);
            var objlog = Devices.CreateLogDevice("objlog", deleteOnClose: true);

            
            var logSettings = new LogSettings
            {
                LogDevice = log,
                ObjectLogDevice = objlog
            };

            logSettings.ReadCacheSettings = new ReadCacheSettings();

            //logSettings.ReadCacheSettings.PageSizeBits = 1024 * 1024 * 10;
            //logSettings.ReadCacheSettings.MemorySizeBits = 1024 * 1024 * 10;

            var faster = new FasterKV<Key, Value, Input, Output, CacheContext, CacheFunctions>(
                    1L << 20,
                    new CacheFunctions(),
                    logSettings,
                    null,
                    new SerializerSettings<Key, Value> { keySerializer = () => new CacheKeySerializer(), valueSerializer = () => new CacheValueSerializer() },
                    new Key()
                );

            //faster.Log.Scan(faster.Log.BeginAddress, faster.Log.TailAddress);
            
            server.Services.Add(GrpcWorker.BindService(new CachingService(faster)));

            server.Start();

        }
    }
}
