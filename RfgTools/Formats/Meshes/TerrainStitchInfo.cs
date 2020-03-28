using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Meshes
{
    public class TerrainStitchInfo
    {
        public vector2f Bmin;
        public vector2f Bmax;
        public uint FilenameOffset;

        public void Read(BinaryReader data)
        {
            Bmin = data.ReadVector2f();
            Bmax = data.ReadVector2f();
            FilenameOffset = data.ReadUInt32();
        }
    }
}
