using System.Linq;
using System.Threading.Tasks;
using Caching.Faster.Workers.Core;
using FASTER.core;
using Google.Protobuf;
using Grpc.Core;
using System;
using Caching.Faster.Worker.Core;
using Caching.Faster.Worker.Core.IdGenerator;

namespace Caching.Faster.Worker
{
    public class CachingService : GrpcWorker.GrpcWorkerBase
    {
        private readonly FasterKV<Key, Value, Input, Output, CacheContext, CacheFunctions> faster;
        private readonly FasterKV<KeyHeader, ValueHeader, KeyHeader, ValueHeader, CacheContext, HeaderCacheFunctions> headers;

        public CachingService(
            FasterKV<Key, Value, Input, Output, CacheContext, CacheFunctions> faster,
            FasterKV<KeyHeader, ValueHeader, KeyHeader, ValueHeader, CacheContext, HeaderCacheFunctions> headers
            )
        {
            this.faster = faster;
            this.headers = headers;
        }

        public long Epoch => (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        public override Task<GetWorkerResponse> Get(GetWorkerRequest request, ServerCallContext context)
        {
            this.faster.StartSession();
            this.headers.StartSession();

            var result = new GetWorkerResponse();

            foreach (var key in request.Key)
            {
                var x = new Common.KeyValuePair()
                {
                    Key = key,
                    Value = ByteString.Empty
                };

                //first lets figure out if the key exists and stil valid
                var k = new KeyHeader();

                var v = new ValueHeader();

                var o = new KeyHeader();

                k.key = key;

                headers.Read(ref k, ref o, ref v, default, 0);


                if (v.epoch > 0)
                {
                    var input = default(Input);

                    var output = default(Output);

                    var key1 = new Key(v.uuid);

                    if (v.epoch > Epoch)
                    {
                        faster.Read(ref key1, ref input, ref output, default, 0);

                        x.Value = ByteString.CopyFrom(output.value.value ?? new byte[] { 0x0 });
                    }
                    else
                    {
                        headers.Delete(ref k, default, 0);
                        faster.Delete(ref key1, default, 0);
                    }
                    
                }
                               
                result.Results.Add(x);
            }

            faster.StopSession();
            headers.StopSession();

            return Task.FromResult(result);
        }

        public override Task<SetWorkerResponse> Set(SetWorkerRequest request, ServerCallContext context)
        {
            this.faster.StartSession();
            this.headers.StartSession();

            var result = new SetWorkerResponse();

            try
            {
                foreach (var key in request.Pairs)
                {
                    var uuid = Id64Generator.GenerateId();

                    var rv = new Value { value = key.Value.ToArray() };

                    var rk = new Key(uuid);

                    var k = new KeyHeader(key.Key);

                    var v = new ValueHeader(uuid, key.Ttl + Epoch);

                    var vs = faster.Upsert(ref rk, ref rv, default, 0);

                    var hs = headers.Upsert(ref k, ref v, default, 0);

                    key.Status = vs != FASTER.core.Status.ERROR && hs != FASTER.core.Status.ERROR;

                    key.Value = ByteString.Empty;

                    result.Results.Add(key);

                }
            }
            catch 
            {
                // log to loki
            }


            faster.StopSession();
            headers.StopSession();

            return Task.FromResult(result);
        }

        public override Task<SetWorkerResponse> Delete(SetWorkerRequest request, ServerCallContext context)
        {
            this.faster.StartSession();
            this.headers.StartSession();

            var result = new SetWorkerResponse();

            foreach (var key in request.Pairs.Select(p => p.Key))
            {
                var x = new Common.KeyValuePair()
                {
                    Key = key,
                    Value = ByteString.Empty
                };

                var k = new KeyHeader(key);

                var v = new ValueHeader();

                var o = new KeyHeader();

                this.headers.Read(ref k, ref o, ref v, default, 0);

                if (v.epoch <= 0) continue;

                var key1 = new Key(v.uuid);

                var hs = this.headers.Delete(ref k, default, 0);
                var vs = this.faster.Delete(ref key1, default, 0);

                x.Status = vs != FASTER.core.Status.ERROR && hs != FASTER.core.Status.ERROR;

                result.Results.Add(x);
            }

            this.faster.StopSession();
            this.headers.StopSession();

            return Task.FromResult(result);
        }

        #region old     
        //public override Task<GetWorkerResponse> Get(GetWorkerRequest request, ServerCallContext context)
        //{

        //    this.faster.StartSession();
        //    var result = new GetWorkerResponse();

        //    foreach (var key in request.Key)
        //    {
        //        var input = default(Input);
        //        var output = default(Output);

        //        var key1 = new Key(key.GetConsistentHashCode());

        //        faster.Read(ref key1, ref input, ref output, default, 0);

        //        var x = new Common.KeyValuePair()
        //        {
        //            Key = key,
        //            Value = ByteString.CopyFrom(output.value.value)
        //        };

        //        result.Results.Add(x);
        //    }
        //    faster.StopSession();
        //    return Task.FromResult(result);
        //}

        //public override Task<SetWorkerResponse> Set(SetWorkerRequest request, ServerCallContext context)
        //{
        //    this.faster.StartSession();
        //    var result = new SetWorkerResponse();
        //    try
        //    {
        //        foreach (var key in request.Pairs)
        //        {

        //            var fvalue = new Value { value = key.Value.ToArray() };

        //            var fkey = new Key(key.Key.GetConsistentHashCode());

        //            var status = faster.Upsert(ref fkey, ref fvalue, default, 0);

        //            key.Status = status != FASTER.core.Status.ERROR;

        //            result.Results.Add(key);

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // log to loki
        //    }


        //    faster.StopSession();
        //    return Task.FromResult(result);
        //}
        #endregion

    }
}
