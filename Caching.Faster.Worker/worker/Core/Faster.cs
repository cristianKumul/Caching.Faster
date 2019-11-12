using FASTER.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Caching.Faster.Workers.Core
{
    public class Faster
    {
    
        public Faster()
        {

        }
        public void test()
        {  
        }

    }
    public class Key : IFasterEqualityComparer<Key>
    {
        public long key;

        public Key() { }

        public Key(long first)
        {
            key = first;
        }

        public long GetHashCode64(ref Key key)
        {
            return Utility.GetHashCode(key.key);
        }
        public bool Equals(ref Key k1, ref Key k2)
        {
            return k1.key == k2.key;
        }
    }

    public class CacheKeySerializer : BinaryObjectSerializer<Key>
    {
        public override void Deserialize(ref Key obj)
        {
            obj.key = reader.ReadInt64();
        }

        public override void Serialize(ref Key obj)
        {
            writer.Write(obj.key);
        }
    }

    public class Value
    {
        public byte[] value;

        public Value() { }

        public Value(byte[] first)
        {
            value = first;
        }
    }

    public class CacheValueSerializer : BinaryObjectSerializer<Value>
    {
        public override void Deserialize(ref Value obj)
        {
            obj.value = reader.ReadBytes((int)reader.BaseStream.Length);
        }

        public override void Serialize(ref Value obj)
        {
            writer.Write(obj.value);
        }
    }

    public struct Input
    {
    }

    public struct Output
    {
        public Value value;
    }

    public struct CacheContext
    {
        public int type;
        public long ticks;
    }

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
            throw new NotImplementedException();
        }

        public void InitialUpdater(ref Key key, ref Input input, ref Value value)
        {
            throw new NotImplementedException();
        }

        public void InPlaceUpdater(ref Key key, ref Input input, ref Value value)
        {
            throw new NotImplementedException();
        }

        public void CheckpointCompletionCallback(Guid sessionId, long serialNum)
        {
            throw new NotImplementedException();
        }

        public void ReadCompletionCallback(ref Key key, ref Input input, ref Output output, CacheContext ctx, Status status)
        {

        }

        public void RMWCompletionCallback(ref Key key, ref Input input, CacheContext ctx, Status status)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void DeleteCompletionCallback(ref Key key, CacheContext ctx)
        {
            throw new NotImplementedException();
        }

        //bool IFunctions<Key, Value, Input, Output, CacheContext>.InPlaceUpdater(ref Key key, ref Input input, ref Value value)
        //{
        //    throw new NotImplementedException();
        //}

        //bool IFunctions<Key, Value, Input, Output, CacheContext>.ConcurrentWriter(ref Key key, ref Value src, ref Value dst)
        //{
        //    dst = src;
        //    return true;
        //}
    }
}