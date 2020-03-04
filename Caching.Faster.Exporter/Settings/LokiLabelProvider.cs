using Serilog.Sinks.Loki.gRPC.Labels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caching.Faster.Exporter.Settings
{
    public class LokiLabelProvider : ILogLabelProvider
    {
        public IList<LokiLabel> GetLabels() => new List<LokiLabel>
        {
            new LokiLabel("app", Environment.GetEnvironmentVariable("LOKI_APPLICATION_NAME") ?? "Faster-Exporter"),
            new LokiLabel("stack", Environment.GetEnvironmentVariable("LOKI_STACK_NAME") ?? "faster-shared"),
            new LokiLabel("env", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development"),
        };
    }
}
