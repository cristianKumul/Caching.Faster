using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            var response = new GetResponse();
            var sw = new Stopwatch();
            sw.Restart();

            await foreach (var p in channeldistribution.GetValuePairs(request.Key, sw))
            {
                //Console.WriteLine($"get: time to get pairs {sw.ElapsedMilliseconds}");
                response.Results.Add(p);
            }

            //Console.WriteLine($"get: writing response {sw.ElapsedMilliseconds}");
            //Console.WriteLine($"\n\n");

            return response;
        }

        public override async Task<SetResponse> Set(SetRequest request, ServerCallContext context)
        {
            var response = new SetResponse();

            await foreach (var p in channeldistribution.SetValuePairs(request.Pairs))
            {
                response.Results.AddRange(p);
            }

            return response;
        }
    }
}
