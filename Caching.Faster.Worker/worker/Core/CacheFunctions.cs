using FASTER.core;
using System;

namespace Caching.Faster.Workers.Core
{
    public class CacheFunctions : IFunctions<Key, Value, Input, Output, CacheContext>
    {
        public void ConcurrentReader(ref Key key, ref Input input, ref Value value, ref Output dst)
        {
            dst.value = value;
        }

        public void ConcurrentWriter(ref Key key, ref Value src, ref Value dst)
        {
            dst = src;
        }

        public void CopyUpdater(ref Key key, ref Input input, ref Value oldValue, ref Value newValue)
        {
            //TODO: throw new NotImplementedException();
        }

        public void InitialUpdater(ref Key key, ref Input input, ref Value value)
        {
            //TODO: throw new NotImplementedException();
        }

        public void InPlaceUpdater(ref Key key, ref Input input, ref Value value)
        {
            //TODO: throw new NotImplementedException();
        }

        public void CheckpointCompletionCallback(Guid sessionId, long serialNum)
        {
            //TODO: throw new NotImplementedException();
        }

        public void ReadCompletionCallback(ref Key key, ref Input input, ref Output output, CacheContext ctx, Status status)
        {

        }

        public void RMWCompletionCallback(ref Key key, ref Input input, CacheContext ctx, Status status)
        {
            //TODO: throw new NotImplementedException();
        }

        public void SingleReader(ref Key key, ref Input input, ref Value value, ref Output dst)
        {
            dst.value = value;
        }

        public void SingleWriter(ref Key key, ref Value src, ref Value dst)
        {
            dst = src;
        }

        public void UpsertCompletionCallback(ref Key key, ref Value value, CacheContext ctx)
        {
           //TODO: throw new NotImplementedException();
        }

        public void DeleteCompletionCallback(ref Key key, CacheContext ctx)
        {
           //TODO: throw new NotImplementedException();
        }

    }
}