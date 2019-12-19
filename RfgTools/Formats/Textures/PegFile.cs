using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RfgTools.Dependencies;

namespace RfgTools.Formats.Textures
{
    public class PegFile
    {
        //Todo: Rename these variables to be closer to their usage and purpose

        public uint Signature;
        public ushort Version;
        public ushort Platform; //Todo: Try to find an enum for this
        public uint DirectoryBlockSize; //Pretty sure this is the cpeg/cvbm file size, gotta check. Todo: Confirm this value is
        public uint DataBlockSize; //Pretty sure this is the gpeg size, gotta check. Todo: Confirm what this value is
        public ushort NumberOfBitmaps; //Todo: Figure out the difference between this and TotalEntries. Might be something to do with animated textures
        public ushort Flags; //Dunno what these do. Never seen them set to anything but 0
        public ushort TotalEntries; //Number of pegs in the file. 
        public ushort AlignValue; //Byte alignment of the (cpu? gpu? both?) peg file

        public List<PegEntry> Entries = new List<PegEntry>();

        private string _cpuFilePath;
        private string _gpuFilePath;

        public string cpuFileName;
        public string gpuFileName;

        public PegFile()
        {

        }

        private void SetAndCheckPaths(string cpuFilePath, string gpuFilePath)
        {
            if (!File.Exists(cpuFilePath))
            {
                throw new Exception($"Could not locate cpu peg file at \"{cpuFilePath}\"");
            }
            if (!File.Exists(gpuFilePath))
            {
                throw new Exception($"Could not locate gpu peg file at \"{gpuFilePath}\"");
            }

            _cpuFilePath = cpuFilePath;
            _gpuFilePath = gpuFilePath;

            cpuFileName = Path.GetFileName(cpuFilePath);
            gpuFileName = Path.GetFileName(gpuFilePath);
        }

        public void Read(string cpuFilePath, string gpuFilePath)
        {
            SetAndCheckPaths(cpuFilePath, gpuFilePath);
            using var cpuFileStream = new FileStream(_cpuFilePath, FileMode.Open);
            using var gpuFileStream = new FileStream(_gpuFilePath, FileMode.Open);
            Read(cpuFileStream, gpuFileStream);
        }

        //Todo: Consider adding warnings for unusual, problematic, or unsupported values. Maybe change some existing exceptions to warnings.
        public void Read(Stream cpuFileStream, Stream gpuFileStream)
        {
            Entries.Clear();
            var header = new BinaryReader(cpuFileStream);
            var data = new BinaryReader(gpuFileStream);

            Signature = header.ReadUInt32();
            if (Signature != 1447773511) //Equals GEKV as a string
            {
                throw new Exception("Header signature must be GEKV. Invalid peg file. Make sure that your packfile extractor didn't incorrectly extract the peg file you're trying to open.");
            }
            Version = header.ReadUInt16();
            if (Version != 10)
            {
                throw new Exception($"Unsupported peg format version detected! Only peg version 10 is supported. Version {Version} was detected");
            }

            Platform = header.ReadUInt16(); //Todo: Add exception or warning for unknown or unsupported platform.
            DirectoryBlockSize = header.ReadUInt32();
            if (header.BaseStream.Length != DirectoryBlockSize)
            {
                throw new Exception($"Error, the size of the header file (cpeg_pc or cvbm_pc) does not match the size value stored in the header! Actual size: {header.BaseStream.Length} bytes, stored size: {DirectoryBlockSize} bytes.");
            }

            DataBlockSize = header.ReadUInt32();
            NumberOfBitmaps = header.ReadUInt16();
            Flags = header.ReadUInt16();
            TotalEntries = header.ReadUInt16();
            AlignValue = header.ReadUInt16();

            //Read peg entries
            for (int i = 0; i < NumberOfBitmaps; i++)
            {
                var entry = new PegEntry();
                entry.Read(header);
                Entries.Add(entry);
            }

            //Read peg entry names
            foreach (var entry in Entries)
            {
                entry.Name = PegUtil.ReadNullTerminatedString(header);
            }

            //Load raw texture data from gpu file, convert to bitmaps for easy handling
            foreach (var entry in Entries)
            {
                entry.RawData = new byte[entry.frame_size];
                data.Read(entry.RawData, 0, (int)entry.frame_size);
                entry.Bitmap = PegUtil.EntryDataToBitmap(entry);
            }
        }

        public void Write(string cpuFilePath, string gpuFilePath)
        {
            SetAndCheckPaths(cpuFilePath, gpuFilePath);
            using var cpuFileStream = new FileStream(_cpuFilePath, FileMode.Truncate);
            using var gpuFileStream = new FileStream(_gpuFilePath, FileMode.Truncate);
            Write(cpuFileStream, gpuFileStream);
        }

        public void Write(Stream cpuFileStream, Stream gpuFileStream)
        {
            var cpuFile = new BinaryWriter(cpuFileStream);
            var gpuFile = new BinaryWriter(gpuFileStream);

            WriteEntryData(gpuFile); //Write entry data to gpu file first so that entry size can be counted.
            WriteHeader(cpuFile);
            WriteEntries(cpuFile);
            WriteEntryNames(cpuFile);

            cpuFile.Seek(8, SeekOrigin.Begin); //Seek to cpu_file and gpu_file size values in cpu_file.
            cpuFile.Write((uint)cpuFile.BaseStream.Length); //Update cpu_file size variable
            cpuFile.Write((uint)gpuFile.BaseStream.Length); //Update gpu_file size variable
        }

        private void WriteHeader(BinaryWriter header)
        {
            header.Write(Signature);
            header.Write(Version);
            header.Write(Platform);
            header.Write(DirectoryBlockSize); //Updated to the correct value in Write() once all the info is gathered
            header.Write(DataBlockSize);
            header.Write(NumberOfBitmaps);
            header.Write(Flags);
            header.Write(TotalEntries);
            header.Write(AlignValue);
        }

        private void WriteEntries(BinaryWriter header)
        {
            foreach (var entry in Entries)
            {
                entry.Write(header);
            }
        }

        private void WriteEntryNames(BinaryWriter header)
        {
            foreach (var entry in Entries)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(entry.Name);
                header.Write(bytes, 0, bytes.Length);
                header.Write((byte)0);
            }
        }

        private void WriteEntryData(BinaryWriter gpuFile)
        {
            foreach (var entry in Entries)
            {
                if (entry.Edited)
                {
                    if (entry.bitmap_format == PegFormat.PC_DXT1)
                    {
                        var compressBuffer = Squish.Compress(entry.RawData, entry.width, entry.height, Squish.Flags.DXT1);
                        gpuFile.Write(compressBuffer);
                        entry.frame_size = (uint)compressBuffer.Length;
                    }
                    else if (entry.bitmap_format == PegFormat.PC_DXT3)
                    {
                        var compressBuffer = Squish.Compress(entry.RawData, entry.width, entry.height, Squish.Flags.DXT3);
                        gpuFile.Write(compressBuffer);
                        entry.frame_size = (uint)compressBuffer.Length;
                    }
                    else if (entry.bitmap_format == PegFormat.PC_DXT5)
                    {
                        var compressBuffer = Squish.Compress(entry.RawData, entry.width, entry.height, Squish.Flags.DXT5);
                        gpuFile.Write(compressBuffer);
                        entry.frame_size = (uint)compressBuffer.Length;
                    }
                    else
                    {
                        throw new Exception($"Unsupported PEG data format detected! {entry.bitmap_format.ToString()} is not yet supported.");
                    }
                }
                else
                {
                    gpuFile.Write(entry.RawData);
                    entry.frame_size = (uint)entry.RawData.Length;
                }
            }
        }
    }
}
