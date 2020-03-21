using System.IO;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Meshes
{
    public class TerrainSubzoneData
    {
        public uint SubzoneIndex;
        public vector3f Position;
        public uint PatchCount;
        public uint PatchesOffset;
        public TerrainRenderableData RenderableData = new TerrainRenderableData();
        public uint NumDecals;
        public uint DecalsOffset;
        public uint StitchMeshDataOffset;
        public uint StitchRenderableOffset;
        public uint NumStitchPieces;
        public uint StitchPiecesOffset;
        public uint NumRoadDecalMeshes;
        public uint RoadDecalMeshesOffset;
        public uint HeaderVersion;
        //996 bytes padding

        public void Read(BinaryReader data)
        {
            SubzoneIndex = data.ReadUInt32();
            Position = data.ReadVector3f();
            PatchCount = data.ReadUInt32();
            PatchesOffset = data.ReadUInt32();
            RenderableData.Read(data);
            NumDecals = data.ReadUInt32();
            DecalsOffset = data.ReadUInt32();
            StitchMeshDataOffset = data.ReadUInt32();
            StitchRenderableOffset = data.ReadUInt32();
            NumStitchPieces = data.ReadUInt32();
            StitchPiecesOffset = data.ReadUInt32();
            NumRoadDecalMeshes = data.ReadUInt32();
            RoadDecalMeshesOffset = data.ReadUInt32();
            HeaderVersion = data.ReadUInt32();
            data.Skip(996);
        }
    }
}
