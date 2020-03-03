using System;
using System.Collections.Generic;
using System.Text;

namespace Caching.Faster.Proxy.Client.Models
{
    public struct KeyValuePair<T>
    {
        public string Key { get; set; }
        public T Value { get; set; }
        public int Ttl { get; set; }
    }
}
