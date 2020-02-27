using FASTER.core;

namespace Caching.Faster.Workers.Core
{
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
}