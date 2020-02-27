using Caching.Faster.Worker.Core.IdGenerator;
using Caching.Faster.Workers.Core;
using FASTER.core;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Caching.Faster.Workers.Extensions.Extensions;
namespace Caching.Faster.Worker.Core
{
    [MessagePackObject]
    public struct KeyHeader : IFasterEqualityComparer<KeyHeader>
    {
        [Key(0)] public string key;
        public KeyHeader(string k)
        {
            key = k;
        }

        public bool Equals(ref KeyHeader k1, ref KeyHeader k2)
        {
            return k1.key == k2.key;
        }

        public long GetHashCode64(ref KeyHeader k)
        {
            return k.key.GetConsistentHashCode();
        }
    }

    [MessagePackObject]
    public struct ValueHeader
    {
        public ValueHeader(long _uuid = -1, long _epoch = -1)
        {
            uuid = _uuid;
            epoch = _epoch;
        }

        [Key(0)] public long uuid;
        [Key(1)] public long epoch;

        public void SetUuid()
        {
       
        }
    }

    public class HeaderCacheFunctions : IFunctions<KeyHeader, ValueHeader, KeyHeader, ValueHeader, CacheContext>
    {
        public void ConcurrentReader(ref KeyHeader key, ref KeyHeader input, ref ValueHeader value, ref ValueHeader dst)
        {
            dst.epoch = value.epoch;
            dst.uuid = value.uuid;
        }

        public void ConcurrentWriter(ref KeyHeader key, ref ValueHeader src, ref ValueHeader dst)
        {
            dst = src;
        }

        public void CopyUpdater(ref KeyHeader key, ref KeyHeader input, ref ValueHeader oldValue, ref ValueHeader newValue)
        {
            throw new NotImplementedException();
        }

        public void InitialUpdater(ref KeyHeader key, ref KeyHeader input, ref ValueHeader value)
        {
            throw new NotImplementedException();
        }

        public void InPlaceUpdater(ref KeyHeader key, ref KeyHeader input, ref ValueHeader value)
        {
            throw new NotImplementedException();
        }

        public void CheckpointCompletionCallback(Guid sessionId, long serialNum)
        {
            throw new NotImplementedException();
        }

        public void ReadCompletionCallback(ref KeyHeader key, ref KeyHeader input, ref ValueHeader output, CacheContext ctx, Status status)
        {

        }

        public void RMWCompletionCallback(ref KeyHeader key, ref KeyHeader input, CacheContext ctx, Status status)
        {
            throw new NotImplementedException();
        }

        public void SingleReader(ref KeyHeader key, ref KeyHeader input, ref ValueHeader value, ref ValueHeader dst)
        {
            dst.epoch = value.epoch;
            dst.uuid = value.uuid;
        }

        public void SingleWriter(ref KeyHeader key, ref ValueHeader src, ref ValueHeader dst)
        {
            dst.epoch = src.epoch;
            dst.uuid = src.uuid;
        }

        public void UpsertCompletionCallback(ref KeyHeader key, ref ValueHeader value, CacheContext ctx)
        {
            throw new NotImplementedException();
        }

        public void DeleteCompletionCallback(ref KeyHeader key, CacheContext ctx)
        {
            throw new NotImplementedException();
        }

    }
}
