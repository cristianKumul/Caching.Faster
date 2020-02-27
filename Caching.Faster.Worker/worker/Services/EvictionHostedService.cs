using Caching.Faster.Worker.Core;
using Caching.Faster.Workers.Core;
using FASTER.core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caching.Faster.Worker.Services
{
    public class EvictionHostedService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly int _scrapeIntervalMinutes = 5;

        private readonly ILogger<EvictionHostedService> _logger;
        private readonly FasterKV<Key, Value, Input, Output, CacheContext, CacheFunctions> values;
        private readonly FasterKV<KeyHeader, ValueHeader, KeyHeader, ValueHeader, CacheContext, HeaderCacheFunctions> headers;

        public long Epoch => (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

        public EvictionHostedService(
            ILogger<EvictionHostedService> logger,
            FasterKV<Key, Value, Input, Output, CacheContext, CacheFunctions> values,
            FasterKV<KeyHeader, ValueHeader, KeyHeader, ValueHeader, CacheContext, HeaderCacheFunctions> headers)
        {
            _logger = logger;
            this.values = values;
            this.headers = headers;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Eviction Service running.");

            _timer = new Timer(DoWork, null, 0, _scrapeIntervalMinutes * 1_000 );

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using var iter = headers.Log.Scan(32, headers.Log.TailAddress);

            while (iter.GetNext(out var info, out var key, out var value))
            {
                if (value.epoch <= Epoch)
                {
                    values.StartSession();
                    headers.StartSession();

                    headers.Delete(ref key, default, 0);

                    var v = new Key(value.uuid);

                    values.Delete(ref v, default, 0);

                    values.StopSession();
                    headers.StopSession();
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Eviction Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
