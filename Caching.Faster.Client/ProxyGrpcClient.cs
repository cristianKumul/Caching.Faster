using Caching.Faster.Common;
using Google.Protobuf;
using Grpc.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caching.Faster.Proxy.Client
{
    public class ProxyGrpcClient : ProxyCache.ProxyCacheClient
    {
        public ProxyGrpcClient(Channel channel) : base(channel)
        {
        }

        public async Task<KeyValuePair> GetKey(string key)
        {
            return (await GetKeys(new[] { key })).FirstOrDefault();
        }

        public async Task<IEnumerable<KeyValuePair>> GetKeys(IEnumerable<string> keys)
        {

            return (await base.GetAsync(keys.GetRequest())).Results;
        }

        public async Task<KeyValuePair> SetKey(string key, string value, int ttl)
        {
            return (await SetKeys(new[] {new KeyValuePair()
                {
                    Key = key,
                    Value = ByteString.CopyFrom(Encoding.UTF8.GetBytes(value)),
                    Ttl = ttl
                }
            })).FirstOrDefault();
        }

        public async Task<IEnumerable<KeyValuePair>> SetKeys(IEnumerable<KeyValuePair> keys)
        {
            return (await base.SetAsync(keys.SetRequest())).Results;
        }
    }
}
