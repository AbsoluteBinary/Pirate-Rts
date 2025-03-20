using System.IO;
using Save_Load_System.Inventory.Helpers;
using UnityEngine;

namespace Save_Load_System.Inventory.Extensions
{
    public static class BinaryReaderExtensions {
        public static SerializableGuid Read(this BinaryReader reader)
        {
            return new SerializableGuid(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32());
        }
    }
}
