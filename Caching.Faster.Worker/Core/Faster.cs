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
            //var log = Devices.CreateLogDevice(Path.GetTempPath() + "hlog.log", deleteOnClose: true);
            //var objlog = Devices.CreateLogDevice(Path.GetTempPath() + "hlog.obj.log", deleteOnClose: true);

            //var logSettings = new LogSettings { LogDevice = log, ObjectLogDevice = objlog };

            //    logSettings.ReadCacheSettings = new ReadCacheSettings();

            //if (cache is null)
            //{
            //    cache = new FasterKV<Key, Value, Input, Output, CacheContext, CacheFunctions>(
            //        1L << 20, 
            //        new CacheFunctions(), 
            //        logSettings,                
            //        null, 
            //        new SerializerSettings<Key, Value> { keySerializer = () => new CacheKeySerializer(), valueSerializer = () => new CacheValueSerializer() }
            //    ); 
            //    cache.StartSession();
            //}

        }
        public void test()
        {  // This sample uses struct key and value types, which are blittable (i.e., do not
           // require a pointer to heap objects). Such datatypes enables the 
           // "high speed" mode for FASTER by using a specialized BlittableAllocator for the
           // hybrid log. You can override the default key equality comparer in two ways;
           // (1) Make Key implement IFasterEqualityComparer<Key> interface
           // (2) Provide IFasterEqualityComparer<Key> instance as param to constructor
           // Note that serializers are not required/used for blittable key and value types.

            // Use Null device



            //Input input = default(Input);
            //Output output = default(Output);

            //var bytes = Encoding.ASCII.GetBytes("hola");

            //var key1 = new Key { k = "13" };
            //var value = new Value { value = bytes, ttl = 24 };

            //// Upsert item into store, and read it back
            //cache.Upsert(ref key1, ref value, Empty.Default, 0);
            //cache.Read(ref key1, ref input, ref output, Empty.Default, 0);

            //if ((output.value.value == value.value) && (output.value.value == value.value))
            //    Console.WriteLine("Sample2: Success!");
            //else
            //    Console.WriteLine("Sample2: Error!");





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

        bool IFunctions<Key, Value, Input, Output, CacheContext>.InPlaceUpdater(ref Key key, ref Input input, ref Value value)
        {
            throw new NotImplementedException();
        }

        bool IFunctions<Key, Value, Input, Output, CacheContext>.ConcurrentWriter(ref Key key, ref Value src, ref Value dst)
        {
            dst = src;
            return true;
        }
    }
}