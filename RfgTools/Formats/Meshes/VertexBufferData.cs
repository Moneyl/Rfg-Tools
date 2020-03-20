using System.IO;
using System.Text;

namespace RfgTools.Formats.Meshes
{
    public class VertexBufferData
    {
        public uint NumVerts;
        public byte VertexStride0;
        public VertexFormat VertexFormat;
        public byte NumUvChannels;
        public byte VertexStride1;
        public uint VertexOffset; //Seems to be a pointer set at runtime

        public void Read(BinaryReader data)
        {
            NumVerts = data.ReadUInt32();
            VertexStride0 = data.ReadByte();
            VertexFormat = (VertexFormat)data.ReadByte();
            NumUvChannels = data.ReadByte();
            VertexStride1 = data.ReadByte();
            VertexOffset = data.ReadUInt32();
        }
    }
}
