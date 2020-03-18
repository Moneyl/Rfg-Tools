using System.IO;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Meshes
{
    public class Vertex
    {
        public vector3f Pos;
        public vector4f Normal;
        public vector4f Tangent;
        public vector4f Uvs;

        public void Read(BinaryReader data)
        {
            Pos = data.ReadVector3f();
            Normal = data.ReadCompressedVector4f();
            Tangent = data.ReadCompressedVector4f();
            Uvs = data.ReadCompressedVector4f();
        }
    }
}
