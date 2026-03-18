using System.IO;
using _Project.Scripts.Inventory.Helpers;

namespace _Project.Scripts.Inventory.Extensions
{
    public static class BinaryReaderExtensions {
        public static SerializableGuid Read(this BinaryReader reader) {
            return new SerializableGuid(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32());
        }
    }
}
