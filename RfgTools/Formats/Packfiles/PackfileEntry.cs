using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RfgTools.Formats.Packfiles
{
    //Each subfile contained in a packfile has one of these entries. Contains info about a subfile.
    public class PackfileEntry
    {
        //Note that all of these offsets are from the start of their respective sections.
        //So, the NameOffset is from the start of the file names block for example.
        public uint NameOffset = 0; //Offset in bytes to files name. First entries value is 0.
        public uint Sector = 0; //Empty. Likely used by game internally.
        public uint DataOffset = 0; //Offset in bytes to this files data in the packfile. Todo: Check to see if this is ever valid, in misc.vpp_pc it seems this is always wrong
        public uint NameHash = 0; //Name hash for this file
        public uint DataSize = 0; //Size in bytes of this files uncompressed data
        public uint CompressedDataSize = 0; //Size in bytes of this files data. Is 0xFFFFFFFF (‭4294967295‬) if not compressed.
        public uint PackagePointer = 0; //Empty. Likely used by game internally.

        public string FullPath; //Used during packing for convenience

        public void ReadFromBinary(BinaryReader file)
        {
            NameOffset = file.ReadUInt32();
            Sector = file.ReadUInt32();
            DataOffset = file.ReadUInt32();
            NameHash = file.ReadUInt32();
            DataSize = file.ReadUInt32();
            CompressedDataSize = file.ReadUInt32();
            PackagePointer = file.ReadUInt32();
        }

        public void WriteToBinary(BinaryWriter file)
        {
            file.Write(NameOffset);
            file.Write(Sector);
            file.Write(DataOffset);
            file.Write(NameHash);
            file.Write(DataSize);
            file.Write(CompressedDataSize);
            file.Write(PackagePointer);
        }
    }
}
