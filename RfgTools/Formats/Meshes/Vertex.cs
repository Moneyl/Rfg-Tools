using System;
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

        public void Read(BinaryReader data, VertexFormat format)
        {
            if (format == VertexFormat.Pixlit1UvNmap)
            {
                Pos = data.ReadVector3f();
                Normal = data.ReadCompressedVector4f();
                Tangent = data.ReadCompressedVector4f();
                Uvs = new vector4f(data.ReadInt16(), data.ReadInt16(), 0.0f, 0.0f);
            }
            else if(format == VertexFormat.Pixlit3UvNmap)
            {
                Pos = data.ReadVector3f();
                Normal = data.ReadCompressedVector4f();
                Tangent = data.ReadCompressedVector4f();
                Uvs = new vector4f(data.ReadInt16(), data.ReadInt16(), 0.0f, 0.0f);
                data.Skip(8); //Skip other UVs, .obj files only support 1 uv per object so for now need to ignore these. These are usually just 0,0 anyway
            }
            else
            {
                throw new Exception($"{format.ToString()} is an unsupported vertex format! Please show the maintainer of this tool this error and the related mesh file.");
            }
        }
    }
}
