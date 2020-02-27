using FASTER.core;

namespace Caching.Faster.Worker.Core
{
    public class CacheKeyHeaderSerializer : BinaryObjectSerializer<KeyHeader>
    {
        public override void Deserialize(ref KeyHeader obj)
        {
            obj.key = reader.ReadString();
        }

        public override void Serialize(ref KeyHeader obj)
        {
            writer.Write(obj.key);
        }
    }
}
