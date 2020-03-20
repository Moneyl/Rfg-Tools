using System.IO;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Meshes
{
    public class SubmeshData
    {
        public int NumRenderBlocks;
        public vector3f OffsetTransform;
        public vector3f Bmin;
        public vector3f Bmax;
        public uint RenderBlocksOffset; //Seems to be a ptr set at runtime

        public void Read(BinaryReader data)
        {
            NumRenderBlocks = data.ReadInt32();
            OffsetTransform = data.ReadVector3f();
            Bmin = data.ReadVector3f();
            Bmax = data.ReadVector3f();
            RenderBlocksOffset = data.ReadUInt32();
            
        }
    }
}
