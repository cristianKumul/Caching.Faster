using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Caching.Faster.Workers.Client
{
    public class GrpcClient
    {
        private readonly GrpcWorker.GrpcWorkerClient client;

        public GrpcClient(GrpcWorker.GrpcWorkerClient client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<KeyValuePair>> GetKeys(IEnumerable<string> keys)
        {
   
            return (await client.GetAsync(keys.GetRequest())).Results;
        }

        public async Task<IEnumerable<KeyValuePair>> SetKeys(IEnumerable<KeyValuePair> keys)
        {
            return (await client.SetAsync(keys.SetRequest())).Results;
        }

        public async Task<IEnumerable<KeyValuePair>> DeleteKeys(IEnumerable<string> keys)
        {
            return (await client.DeleteAsync(keys.GetRequest())).Results;
        }
    }
}
