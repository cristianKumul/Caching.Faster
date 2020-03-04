using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Caching.Faster.Exporter.Services
{
    public class MetricsHostedService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly int _scrapeIntervalMinutes = 1;
        private readonly ILogger<MetricsHostedService> _logger;

        public MetricsHostedService(ILogger<MetricsHostedService> logger)
        {
            _logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Metrics Hosted Service runing.");
            _timer = new Timer(Execute, null, 0, _scrapeIntervalMinutes * 60_000);
            return Task.CompletedTask;
        }

        private void Execute(object state)
        {
            for (int i = 0; i < 10; i++)
            {
                _logger.LogInformation($"{i} times");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Metrics Hosted is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
