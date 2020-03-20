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

        public void Read(BinaryReader data)
        {
            MaterialMapIndex = data.ReadUInt16();
            data.Skip(2);
            StartIndex = data.ReadUInt32();
            NumIndices = data.ReadUInt32();
            MinIndex = data.ReadUInt32();
            MaxIndex = data.ReadUInt32();
        }
    }
}