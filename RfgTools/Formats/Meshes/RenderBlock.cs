using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RfgTools.Helpers;

namespace RfgTools.Formats.Meshes
{
    public class RenderBlock
    {
        public ushort MaterialMapIndex;
        //2 bytes padding
        public uint StartIndex;
        public uint NumIndices;
        public uint MinIndex;
        public uint MaxIndex;

        public void Read(BinaryReader file)
        {
            MaterialMapIndex = file.ReadUInt16();
            file.Skip(2);
            StartIndex = file.ReadUInt32();
            NumIndices = file.ReadUInt32();
            MinIndex = file.ReadUInt32();
            MaxIndex = file.ReadUInt32();
        }
    }
}