using FASTER.core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Caching.Faster.Workers.Core
{
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

    public class Value
    {
        public byte[] value;

        public Value() { }

        public Value(byte[] first)
        {
            value = first;
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
}