using System.Linq;
using System.Threading.Tasks;
using Caching.Faster.Workers.Core;
using Caching.Faster.Workers.Extensions;
using Caching.Faster.Workers;
using FASTER.core;
using Google.Protobuf;
using Grpc.Core;

namespace Caching.Faster.Worker
{
    public class CachingService : GrpcWorker.GrpcWorkerBase
    {
        private readonly FasterKV<Key, Value, Input, Output, CacheContext, CacheFunctions> faster;

        public CachingService(FasterKV<Key, Value, Input, Output, CacheContext, CacheFunctions> faster)
        {
            this.faster = faster;
            this.faster.StartSession();
        }

        public override Task<GetWorkerResponse> Get(GetWorkerRequest request, ServerCallContext context)
        {
            var result = new GetWorkerResponse();
            
            foreach (var key in request.Key)
            {
                var input = default(Input);
                var output = default(Output);

                var key1 = new Key(key.GetConsistentHashCode());

                faster.Read(ref key1, ref input, ref output, default, 0);

                var x = new KeyValuePair() {
                    Key = key,
                    Value = ByteString.CopyFrom(output.value.value)
                };

                result.Results.Add(x);
            }

            return Task.FromResult(result);
        }

        public override Task<SetWorkerResponse> Set(SetWorkerRequest request, ServerCallContext context)
        {
            var result = new SetWorkerResponse();
            try
            {
                foreach (var key in request.Pairs)
                {

                    var fvalue = new Value { value = key.Value.ToArray() };

                    var fkey = new Key(key.Key.GetConsistentHashCode());

                    var status = faster.Upsert(ref fkey, ref fvalue, default, 0);

                    key.Status = status != FASTER.core.Status.ERROR;

                    result.Results.Add(key);

                }
            }
            catch 
            {
               // log to loki
            }


            faster.StopSession();
            return Task.FromResult(result);
        }
    }
}
