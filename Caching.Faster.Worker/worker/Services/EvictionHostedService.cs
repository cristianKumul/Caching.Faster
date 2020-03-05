using Caching.Faster.Worker.Collectors;
using Caching.Faster.Worker.Core;
using Caching.Faster.Workers.Core;
using FASTER.core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
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
        private readonly int _scrapeIntervalMinutes = Convert.ToInt32(Environment.GetEnvironmentVariable("SCRAPE_INTERVAL_MINUTES"));

        private readonly ILogger<EvictionHostedService> _logger;
        private readonly FasterKV<Key, Value, Input, Output, CacheContext, CacheFunctions> values;
        private readonly FasterKV<KeyHeader, ValueHeader, KeyHeader, ValueHeader, CacheContext, HeaderCacheFunctions> headers;
        private readonly EvictedMetric evictedMetric;

        public long Epoch => (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

        public EvictionHostedService(
            ILogger<EvictionHostedService> logger,
            FasterKV<Key, Value, Input, Output, CacheContext, CacheFunctions> values,
            FasterKV<KeyHeader, ValueHeader, KeyHeader, ValueHeader, CacheContext, HeaderCacheFunctions> headers,
            EvictedMetric evictedMetric)
        {
            _logger = logger;
            this.values = values;
            this.headers = headers;
            this.evictedMetric = evictedMetric;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Eviction Service running.");

            _timer = new Timer(DoWork, null, 0, _scrapeIntervalMinutes * 60_000 );

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

                    evictedMetric.EvictedKeysByHostedService();
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
