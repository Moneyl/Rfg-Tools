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

        public void Read(BinaryReader cpuFile)
        {
            NumRenderBlocks = cpuFile.ReadInt32();
            OffsetTransform = cpuFile.ReadVector3f();
            Bmin = cpuFile.ReadVector3f();
            Bmax = cpuFile.ReadVector3f();
            RenderBlocksOffset = cpuFile.ReadUInt32();
            
        }
    }
}
