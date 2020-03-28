using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RfgTools.Formats.Meshes
{
    public class TerrainLayerMap
    {
        public uint ResX;
        public uint ResY;
        public uint BitDepth;
        public uint DataSize;
        public uint DataOffset;
        public uint NumMaterials;
        public uint MaterialNamesOffset;
        public uint MaterialIndexOffset;

        public void Read(BinaryReader data)
        {
            ResX = data.ReadUInt32();
            ResY = data.ReadUInt32();
            BitDepth = data.ReadUInt32();
            DataSize = data.ReadUInt32();
            DataOffset = data.ReadUInt32();
            NumMaterials = data.ReadUInt32();
            MaterialNamesOffset = data.ReadUInt32();
            MaterialIndexOffset = data.ReadUInt32();
        }
    }
}
