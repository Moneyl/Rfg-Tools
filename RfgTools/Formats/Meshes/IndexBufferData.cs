using System.IO;
using System.Text;

namespace RfgTools.Formats.Meshes
{
    public class IndexBufferData
    {
        public uint NumIndices;
        public uint IndicesOffset; //Seems to be a pointer set at runtime
        public byte IndexSize;
        public PrimitiveTopology PrimitiveType; //Todo: Check if this is really the PrimitiveType enum by looking at a bunch of meshes or the data
        public ushort NumBlocks;

        public void Read(BinaryReader data)
        {
            NumIndices = data.ReadUInt32();
            IndicesOffset = data.ReadUInt32();
            IndexSize = data.ReadByte();
            PrimitiveType = (PrimitiveTopology)data.ReadByte();
            NumBlocks = data.ReadUInt16();
        }
    }
}
