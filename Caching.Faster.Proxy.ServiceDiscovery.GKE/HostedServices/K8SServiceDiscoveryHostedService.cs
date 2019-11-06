using Caching.Faster.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Caching.Faster.Proxy.ServiceDiscovery.GKE.HostedServices
{
    public class K8SServiceDiscoveryHostedService : IHostedService, IDisposable
    {
        private readonly int _scrapeInterval = 15;
        private int _executionCount = 0;

        private readonly ILogger<K8SServiceDiscoveryHostedService> _logger;
        private readonly K8SServiceDiscovery _serviceDiscovery;

        private Timer _timer;

        public static event EventHandler<FasterWorkers> OnDiscoveryCompleted;
        public K8SServiceDiscoveryHostedService(ILogger<K8SServiceDiscoveryHostedService> logger, K8SServiceDiscovery serviceDiscovery)
        {
            _logger = logger;
            _serviceDiscovery = serviceDiscovery;
            _serviceDiscovery.OnDiscoveryCompleted += (s, w) => OnDiscoveryCompleted?.Invoke(s, w);
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("K8S Discovery Service running.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(_scrapeInterval));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            //lets set a max count to 60000 then reset
            if (_executionCount > 60000)
                _executionCount = 0;

            if (_executionCount % 60 == 0)
            {
                _serviceDiscovery.DiscoverNamespaces();
            }

            if (_executionCount % 8 == 0)
            {
                _serviceDiscovery.DiscoverWorkers();
            }
            else
            {
                _serviceDiscovery.RefreshWorkers();
            }
                       
            _executionCount++;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("K8S Discovery Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
