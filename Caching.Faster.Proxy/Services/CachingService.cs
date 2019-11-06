using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.Faster.Proxy.Hashing;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Caching.Faster.Proxy
{
    public class CachingService : Cache.CacheBase
    {
        private readonly ILogger<CachingService> logger;
        private readonly ChannelDistribution channelDistribution;

        public CachingService(ILogger<CachingService> logger, ChannelDistribution channelDistribution)
        {
            this.logger = logger;
            this.channelDistribution = channelDistribution;
        }

        public override async Task<GetResponse> Get(GetRequest request, ServerCallContext context)
        {
            var pairs = await channelDistribution.GetValuePairs(request.Key.ToArray());

            var response = new GetResponse();

            response.Results.AddRange(pairs.Select(p => new KeyValuePair() { Key = p.Key, Value = p.Value }));

            return response;
        }

        public override async Task<SetResponse> Set(SetRequest request, ServerCallContext context)
        {
            var pairs = await channelDistribution.SetValuePairs(request.Pairs.ToArray());

            var response = new SetResponse();

            response.Results.AddRange(pairs.Select(p => new KeyValuePair() { Key = p.Key, Status = p.Status }));

            return response;
        }
    }
}
