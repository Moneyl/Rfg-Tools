using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RfgTools.Formats.Packfiles
{
    public class PackfileHeader
    {
        //Header block
        public uint Signature = 0; //Magic sig for this format
        public uint Version = 0; //V3 is used in all versions of RFG
        public char[] ShortName = { }; //65 Bytes //Empty by default
        public char[] PathName = { }; //256 Bytes //Empty by default
        //3 Bytes Padding 
        public uint Flags = 0; //Values of Compressed and Condensed stored here in file.
        public bool Compressed = false; //Stored in flags in file, only here for convenience
        public bool Condensed = false; //Stored in flags in file, only here for convenience
        public uint Sector = 0; //Empty. Likely used by game internally.
        public uint NumberOfFiles = 0; //Number of files in the packfile
        public uint FileSize = 0; //Total size of the packfile in bytes
        public uint DirectoryBlockSize = 0; //Size of the packfiles directory block in bytes
        public uint FilenameBlockSize = 0; //Size of the packfiles file names block in bytes
        public uint DataSize = 0; //Uncompressed size of the packfiles data block in bytes
        public uint CompressedDataSize = 0; //Compressed size of the packfiles data block in bytes. Is 0xFFFFFFFF (‭4294967295‬) if not compressed. 
        public uint DirectoryPointer = 0; //Empty. Likely used by game internally.
        public uint FilenamesPointer = 0; //Empty. Likely used by game internally.
        public uint DataPointer = 0; //Empty. Likely used by game internally.
        public uint OpenCount = 0; //Empty. Likely used by game internally.
        //1668 Bytes Padding

        public void ReadFromBinary(BinaryReader file)
        {
            ShortName = new char[65];
            PathName = new char[256];

            Signature = file.ReadUInt32();
            if (Signature != 1367935694) //Hex: 0xCE0A8951
            {
                throw new Exception("Error! Invalid packfile signature");
            }
            Version = file.ReadUInt32();
            if (Version != 3)
            {
                throw new Exception("Error! Invalid packfile version. Expected version 3. The detected version is " + Version);
            }
            ShortName = file.ReadChars(65);
            PathName = file.ReadChars(256);
            file.ReadBytes(3); //3 Bytes padding
            Flags = file.ReadUInt32();
            if (Flags == 1)
            {
                Compressed = true;
            }
            if (Flags == 2)
            {
                Condensed = true;
            }
            if (Flags == 3)
            {
                Compressed = true;
                Condensed = true;
            }
            Sector = file.ReadUInt32();
            NumberOfFiles = file.ReadUInt32();
            FileSize = file.ReadUInt32();
            DirectoryBlockSize = file.ReadUInt32();
            FilenameBlockSize = file.ReadUInt32();
            DataSize = file.ReadUInt32();
            CompressedDataSize = file.ReadUInt32();
            DirectoryPointer = file.ReadUInt32();
            FilenamesPointer = file.ReadUInt32();
            DataPointer = file.ReadUInt32();
            OpenCount = file.ReadUInt32();
            file.ReadBytes(1668); //Padding to next chunk/block. VPPs are in made up of 2048 byte aligned chunks.
        }

        public void WriteToBinary(BinaryWriter file)
        {
            file.Write(Signature);
            file.Write(Version);
            file.Write(ShortName); //Make sure this is 65 bytes before writing
            file.Write(PathName); //Make sure this is 256 bytes before writing
            file.Write(new byte[] { 0x0, 0x0, 0x0 }); //3 Bytes padding
            file.Write(Flags);
            file.Write(Sector);
            file.Write(NumberOfFiles);
            file.Write(FileSize);
            file.Write(DirectoryBlockSize);
            file.Write(FilenameBlockSize);
            file.Write(DataSize);
            file.Write(CompressedDataSize);
            file.Write(DirectoryPointer);
            file.Write(FilenamesPointer);
            file.Write(DataPointer);
            file.Write(OpenCount);
            for (int i = 0; i < 1668; i++) //Write padding to next chunk/block.
            {
                file.Write((byte)0x0);
            }
        }
    }
}
