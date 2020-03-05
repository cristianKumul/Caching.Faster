using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BestDay.Prometheus.AspNetCore.Extensions.Tracking;
using Caching.Faster.Proxy.Hashing;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Caching.Faster.Proxy
{
    public class CachingService : ProxyCache.ProxyCacheBase
    {
        private readonly ChannelDistribution channeldistribution;


        public CachingService(ChannelDistribution channeldistribution)
        {
            this.channeldistribution = channeldistribution;

        }

        public override async Task<GetResponse> Get(GetRequest request, ServerCallContext context)
        {
            var tracker = new Tracker(ref context); 
            var response = new GetResponse();
            var sw = Stopwatch.StartNew();

            await foreach (var p in channeldistribution.GetValuePairs(request.Key))
            {
                response.Results.Add(p);
            }

            tracker.TrackDependency("FasterWorker", "Get", sw.Elapsed.TotalSeconds);

            return response;
        }

        public override async Task<SetResponse> Set(SetRequest request, ServerCallContext context)
        {
            var tracker = new Tracker(ref context);
            var response = new SetResponse();
            var sw = Stopwatch.StartNew();

            await foreach (var p in channeldistribution.SetValuePairs(request.Pairs))
            {
                response.Results.AddRange(p);
            }

            tracker.TrackDependency("FasterWorker", "Set", sw.Elapsed.TotalSeconds);

            return response;
        }

        public override async Task<SetResponse> Delete(SetRequest request, ServerCallContext context)
        {
            var tracker = new Tracker(ref context);
            var response = new SetResponse();
            var sw = Stopwatch.StartNew();

            await foreach (var p in channeldistribution.DeleteValuePairs(request.Pairs))
            {
                response.Results.AddRange(p);
            }

            tracker.TrackDependency("FasterWorker", "Delete", sw.Elapsed.TotalSeconds);

            return response;
        }

    }
}
