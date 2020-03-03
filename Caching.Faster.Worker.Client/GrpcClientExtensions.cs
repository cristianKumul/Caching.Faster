using System.Collections.Generic;
using System.Linq;

namespace Caching.Faster.Workers.Client
{
    public static class GrpcClientExtensions
    {
        public static GetWorkerRequest GetRequest(this IEnumerable<string> keys)
        {
            var request = new GetWorkerRequest();

            request.Key.AddRange(keys);
            
            return request;
        }

        public static SetWorkerRequest SetRequest(this IEnumerable<KeyValuePair> keys)
        {
            var request = new SetWorkerRequest();

            request.Pairs.AddRange(keys);

            return request;
        }

        public static SetWorkerRequest SetRequest(this IEnumerable<string> keys)
        {
            var request = new SetWorkerRequest();

            request.Pairs.AddRange( keys.Select(p => new KeyValuePair() { Key = p}));

            return request;
        }

        public static IEnumerable<KeyValuePair>  GetKeyValuePair (this GetWorkerResponse response)
        {
            if (response.Results.Count > 0)
            {
                foreach (var pair in response.Results)
                {
                    yield return pair;
                    
                }
            }
        }
    }
}
