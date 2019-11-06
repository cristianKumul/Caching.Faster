using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caching.Faster.Proxy.Hashing
{
    public class ChannelDistribution
    {
        private readonly ConsistentHash consistentHash;

        public ChannelDistribution(ConsistentHash consistentHash)
        {
            this.consistentHash = consistentHash;
        }

        public async ValueTask<IEnumerable<Workers.KeyValuePair>> GetValuePairs(IEnumerable<string> keys)
        {
            var c = new Dictionary<string, Workers.Client.GrpcClient>();
            var k = new Dictionary<string, List<string>>();

            foreach (var key in keys)
            {
                var node = consistentHash.GetNode(key);
                var client = consistentHash.GetGrpcChannel(node);

                c.TryAdd(node.Address, client);

                if (k.TryGetValue(node.Address, out var list))
                {
                    list.Add(key);
                }
                else
                {
                    k.Add(node.Address, new List<string>() { key });
                }

            }
            var tasks = new List<Task<IEnumerable<Workers.KeyValuePair>>>();

            foreach (var pair in c)
            {
                var ks = k[pair.Key] as IEnumerable<string>;

                tasks.Add(pair.Value.GetKeys(ks));

            }

            var res = await Task.WhenAll(tasks);

            return res.SelectMany(p => p);

        }

        public async ValueTask<IEnumerable<Workers.KeyValuePair>> SetValuePairs(IEnumerable<KeyValuePair> keys)
        {
            var c = new Dictionary<string, Workers.Client.GrpcClient>();
            var k = new Dictionary<string, List<KeyValuePair>>();

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
                    k.Add(node.Address, new List<KeyValuePair>() { key });
                }

            }
            var tasks = new List<Task<IEnumerable<Workers.KeyValuePair>>>();

            foreach (var pair in c)
            {
                var ks = k[pair.Key] as IEnumerable<Workers.KeyValuePair>;

                tasks.Add(pair.Value.SetKeys(ks));

            }

            var res = await Task.WhenAll(tasks);

            return res.SelectMany(p => p);

        }
    }
}
