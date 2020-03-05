using Caching.Faster.Workers.Client;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Caching.Faster.Worker.GrpcWorker;

namespace Caching.Faster.Proxy.Hashing
{
    public class ChannelDistribution
    {
        private readonly ConsistentHash consistentHash;

        public ChannelDistribution(ConsistentHash consistentHash)
        {
            this.consistentHash = consistentHash;
        }

        public async IAsyncEnumerable<Common.KeyValuePair> GetValuePairs(IEnumerable<string> keys)
        {
            await foreach (var item in GetValuePairs3(keys))
            {
                foreach (var keypair in item)
                {
                    yield return keypair;
                }
            }

        }
        public async IAsyncEnumerable<IEnumerable<Common.KeyValuePair>> GetValuePairs3(IEnumerable<string> keys)
        {
            var k = new Dictionary<string, List<string>>();

            foreach (var key in keys)
            {
                var node = consistentHash.GetNode(key);

                if (k.TryGetValue(node.Address, out var list))
                {
                    list.Add(key);
                }
                else
                {
                    k.Add(node.Address, new List<string>() { key });
                }

            }

            foreach (var key in k)
            {
                var vs = new Worker.GetWorkerRequest();
                vs.Key.AddRange(key.Value);
                var rs = await consistentHash.GetGrpcChannel(consistentHash.GetNode(key.Value[0])).GetAsync(vs);
                yield return rs.Results;
            }
        }


        public async IAsyncEnumerable<IEnumerable<Common.KeyValuePair>> SetValuePairs(IEnumerable<Common.KeyValuePair> keys)
        {

            var c = new Dictionary<string, GrpcWorkerClient>();
            var k = new Dictionary<string, List<Common.KeyValuePair>>();

            foreach (var key in keys)
            {
                var node = consistentHash.GetNode(key.Key);
                var client = consistentHash.GetGrpcChannel(node);

                c.TryAdd(node.Address, client);

                if (k.TryGetValue(node.Address, out var list))
                {
                    list.Add(key);
                }
                else
                {
                    k.Add(node.Address, new List<Common.KeyValuePair>() { key });
                }

            }

            foreach (var pair in c)
            {
                var x = new Worker.SetWorkerRequest();

                x.Pairs.AddRange(k[pair.Key]);

                var rs = await pair.Value.SetAsync(x);

                yield return rs.Results;
            }
        }

        public async IAsyncEnumerable<IEnumerable<Common.KeyValuePair>> DeleteValuePairs(IEnumerable<Common.KeyValuePair> keys)
        {

            var c = new Dictionary<string, GrpcWorkerClient>();
            var k = new Dictionary<string, List<Common.KeyValuePair>>();

            foreach (var key in keys)
            {
                var node = consistentHash.GetNode(key.Key);
                var client = consistentHash.GetGrpcChannel(node);

                c.TryAdd(node.Address, client);

                if (k.TryGetValue(node.Address, out var list))
                {
                    list.Add(key);
                }
                else
                {
                    k.Add(node.Address, new List<Common.KeyValuePair>() { key });
                }

            }

            foreach (var pair in c)
            {
                var x = new Worker.SetWorkerRequest();

                x.Pairs.AddRange(k[pair.Key]);

                var rs = await pair.Value.DeleteAsync(x);

                yield return rs.Results;
            }
        }
    }
}
