using Prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caching.Faster.Worker.Collectors
{
    public class EvictedMetric
    {
        private readonly static Counter Evicted = Metrics.CreateCounter("faster_total_keys_evicted", "Total of keys evicted", new CounterConfiguration()
        {
            LabelNames = new[] { "method" }
        });

        public void EvictedKeysByHostedService()
        {
            Evicted.WithLabels("hosted-service").Inc();
        }

        public void EvictedKeysByExpiration()
        {
            Evicted.WithLabels("expiration").Inc();
        }
    }
}
