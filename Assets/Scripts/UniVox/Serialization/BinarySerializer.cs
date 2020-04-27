using System.IO;

namespace UniVox.Serialization
{
    public abstract class BinarySerializer<T>
    {
        public abstract void Serialize(BinaryWriter writer, T data);
        public abstract T Deserialize(BinaryReader reader);
    }
}