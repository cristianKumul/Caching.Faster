using FASTER.core;
using MessagePack;
using System.IO;

namespace Caching.Faster.Worker.Core
{
    public class CacheValueHeaderSerializer : IObjectSerializer<ValueHeader>
    {
        protected BinaryReader reader;
        protected BinaryWriter writer;

        public void BeginDeserialize(Stream stream)
        {
            reader = new BinaryReader(stream);
        }

        public void EndDeserialize()
        {
        }

        public void BeginSerialize(Stream stream)
        {
            writer = new BinaryWriter(stream);
        }

        public void EndSerialize()
        {
            writer.Dispose();
        }

        public void Deserialize(ref ValueHeader obj)
        {
            var header = MessagePackSerializer.Deserialize<ValueHeader>(reader.BaseStream);

            obj.epoch = header.epoch;
            obj.uuid = header.uuid;
        }

        public void Serialize(ref ValueHeader obj)
        {
            writer.Write(MessagePackSerializer.Serialize(obj));
        }
    }
}
