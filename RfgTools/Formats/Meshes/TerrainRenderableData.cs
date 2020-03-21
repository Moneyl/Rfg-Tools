using System.IO;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Meshes
{
    public class TerrainRenderableData
    {
        public uint MeshDataOffset;
        public uint RenderableOffset;
        public vector3f AabbMin;
        public vector3f AabbMax;
        public vector3f BspherePosition;
        public float BsphereRadius;

        public void Read(BinaryReader data)
        {
            MeshDataOffset = data.ReadUInt32();
            RenderableOffset = data.ReadUInt32();
            AabbMin = data.ReadVector3f();
            AabbMax = data.ReadVector3f();
            BspherePosition = data.ReadVector3f();
            BsphereRadius = data.ReadSingle();
        }
    }
}
