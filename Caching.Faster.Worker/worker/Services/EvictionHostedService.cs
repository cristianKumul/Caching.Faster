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
        private readonly int _chunkSize = Convert.ToInt32(Environment.GetEnvironmentVariable("SCRAPE_CHUNK_SIZE"));

        private readonly ILogger<EvictionHostedService> _logger;
        private readonly FasterKV<Key, Value, Input, Output, CacheContext, CacheFunctions> values;
        private readonly FasterKV<KeyHeader, ValueHeader, KeyHeader, ValueHeader, CacheContext, HeaderCacheFunctions> headers;
        private readonly EvictedMetric evictedMetric;
        private readonly static Gauge totalKeys = Metrics.CreateGauge("faster_total_keys", "Number of keys");

        private static long currentScrappedMemory = 32;

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

            _timer = new Timer(DoWork, null, 0, _scrapeIntervalMinutes * 10_000 );//todo

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using var iter = GetIterator();
            int iterations = 0;
            while (iter.GetNext(out var info, out var key, out var value) && iterations <= _chunkSize)
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

                currentScrappedMemory = iter.CurrentAddress;

                iterations++;
            }
            totalKeys.Set((headers.Log.TailAddress - 32) / 32);

            _logger.LogDebug("Last iteration {currentScrappedMemory}", currentScrappedMemory);
            _logger.LogDebug("Last tail address {tailAddress}", headers.Log.TailAddress);
            _logger.LogDebug($"Total Keys {(headers.Log.TailAddress - 32) / 32}");
        }

        private IFasterScanIterator<KeyHeader,ValueHeader> GetIterator()
        {
            if (currentScrappedMemory == headers.Log.TailAddress)
            {
                currentScrappedMemory = 32;
            }

            return headers.Log.Scan(currentScrappedMemory, headers.Log.TailAddress);
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
