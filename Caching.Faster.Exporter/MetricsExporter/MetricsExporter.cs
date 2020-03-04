using Prometheus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caching.Faster.Exporter.Exporter
{
    class MetricsExporter
    {
        public Counter PrometheusCounter { get; set; }
        public Histogram PrometheusHistogram { get; set; }
        public Histogram PrometheusHistogramIntern { get; set; }

        public MetricsExporter()
        {

            this.PrometheusCounter = Metrics.CreateCounter($"{this.resteloptions.Prefixmetrics}_excepciones", "Contador de excepciones por hoteles", new Prometheus.CounterConfiguration
            {
                LabelNames = new[] { "hotel", "interface", "stage" },
                SuppressInitialValue = false,
            });
            this.PrometheusHistogram = Metrics.CreateHistogram($"{this.resteloptions.Prefixmetrics}_duration_seconds_provider", "The duration in seconds between the response to a request.",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.01, 2, 10),
                LabelNames = new[] { "status_code", "method" },
            });
            this.PrometheusHistogramIntern = Metrics.CreateHistogram($"{this.resteloptions.Prefixmetrics}_duration_seconds_postasync", "The duration in seconds between the response to a request.",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.01, 2, 10),
                LabelNames = new[] { "status_code", "method" },
            });
        }

        public void RegisterMetric(int hotel, string stage)
        {
            this.PrometheusCounter.WithLabels(new[] { hotel.ToString(), "hotelbeds", stage }).Inc();
        }
        public void RegisterResponseTime(int statusCode, string method, TimeSpan elapsed)
        {
            this.PrometheusHistogram.Labels(statusCode.ToString(), method).Observe(elapsed.TotalSeconds);
        }
        public void RegisterResponseInternTime(int statusCode, string method, TimeSpan elapsed)
        {
            this.PrometheusHistogramIntern.Labels(statusCode.ToString(), method).Observe(elapsed.TotalSeconds);
        }
    }
}

