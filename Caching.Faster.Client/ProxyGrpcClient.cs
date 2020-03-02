using Caching.Faster.Common;
using Caching.Faster.Proxy.Client.Models;
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

        public async Task<T> GetKey<T>(string key)
        {   
            return (await GetKeys<T>(new[] { key })).FirstOrDefault();
        }

        public async Task<IEnumerable<T>> GetKeys<T>(IEnumerable<string> keys)
        {
            var results = (await base.GetAsync(keys.GetRequest())).Results;
            return results.Where(keyValue => keyValue.Value.Length > 0).Select(keyValue => {
                return Utf8Json.JsonSerializer.Deserialize<T>(keyValue.Value.ToStringUtf8());
            });
        }

        public async Task<KeyValuePair> SetKey<T>(string key, T value, int ttl)
        {
            return (await SetKeys(new[] { new KeyValuePair<T>()
                {
                    Key = key,
                    Value = value,
                    Ttl = ttl
                }
            })).FirstOrDefault();
        }

        public async Task<IEnumerable<KeyValuePair>> SetKeys<T>(IEnumerable<KeyValuePair<T>> keys)
        {
            return (await base.SetAsync(keys.SetRequest())).Results;
        }
    }
}
