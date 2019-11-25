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
        //Offset in bytes to this files data in the packfile. Stored as a long internally so tools can actually use it,
        //but read/wrote as uint since the game expects a 4 byte value
        public long DataOffset = 0; 
        public uint NameHash = 0; //Name hash for this file
        public uint DataSize = 0; //Size in bytes of this files uncompressed data
        public uint CompressedDataSize = 0; //Size in bytes of this files data. Is 0xFFFFFFFF (‭4294967295‬) if not compressed.
        public uint PackagePointer = 0; //Empty. Likely used by game internally.

        //Convenience variables used for tooling
        public string FullPath; 
        public string FileName;
        public string Extension;

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
            //Calculate value as if it was always a uint and the data wrapped.
            //Done this way to attempt to emulate what the game originally stored in files.
            ulong finalDataOffset = DataSize;
            while (finalDataOffset > uint.MaxValue) //Todo: Double check that this matches what the game expects
            {
                finalDataOffset -= uint.MaxValue;
            }

            file.Write(NameOffset);
            file.Write(Sector);
            file.Write((uint)finalDataOffset);
            file.Write(NameHash);
            file.Write(DataSize);
            file.Write(CompressedDataSize);
            file.Write(PackagePointer);
        }
    }
}
