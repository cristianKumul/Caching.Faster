using FASTER.core;

namespace Caching.Faster.Workers.Core
{
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
}