using System.IO;

namespace RfgTools.Formats.Meshes
{
    public class TextureDesc
    {
        public uint NameOffset;
        public uint NameChecksum; //Todo: Confirm this is actually a checksum and figure out how to calc & validate it
        public uint TextureIndex;

        public void Read(BinaryReader data)
        {
            NameOffset = data.ReadUInt32();
            NameChecksum = data.ReadUInt32();
            TextureIndex = data.ReadUInt32();
        }
    }
}
