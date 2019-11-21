using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace RfgTools.Formats.Packfiles
{
    public class Packfile
    {
        //Header block
        public PackfileHeader Header;

        //Directory block
        public List<PackfileEntry> DirectoryEntries;

        //Filenames block
        public List<string> Filenames;

        //Data below here is only used by the unpacker
        public bool Verbose = false;

        public uint DataStartOffset = 0;

        public Packfile(bool verbose)
        {
            Verbose = verbose;
        }

        /*
            Reads the header, directory, and filenames blocks from the file. Doesn't read actual sub-file data
            Useful for quick initial analysis of a packfile for use with tools, so they can see what's inside 
            without loading the bulk of the data.
        */
        public void ReadMetadata(string packfilePath)
        {
            string packfileName = Path.GetFileName(packfilePath);
            var packfileInfo = new FileInfo(packfilePath);
            Console.WriteLine("Extracting " + packfileName + "...");
            if (packfileInfo.Length <= 2048)
            {
                Console.WriteLine("Cancelled extraction of {0}. Packfile is empty!", packfileName);
                return;
            }
            if (Verbose)
            {
                Console.WriteLine(packfileName + "> Reading header data...");
            }

            var packfile = new BinaryReader(new FileStream(packfilePath, FileMode.Open));
            Header = new PackfileHeader();
            Header.ReadFromBinary(packfile);

            DirectoryEntries = new List<PackfileEntry>();
            for (int i = 0; i < Header.NumberOfFiles; i++)
            {
                var entry = new PackfileEntry();
                entry.ReadFromBinary(packfile);
                DirectoryEntries.Add(entry);
            }
            packfile.ReadBytes(2048 - ((int)packfile.BaseStream.Position % 2048)); //Alignment Padding

            Filenames = new List<string>();
            for (int i = 0; i < Header.NumberOfFiles; i++)
            {
                var name = new StringBuilder();
                do
                {
                    name.Append(packfile.ReadChar());
                }
                while (packfile.PeekChar() != 0);
                Filenames.Add(name.ToString());
                packfile.ReadByte(); //Move past null byte
            }
            packfile.ReadBytes(2048 - ((int)packfile.BaseStream.Position % 2048)); //Alignment Padding
            DataStartOffset = (uint)packfile.BaseStream.Position;
        }

        public void ExtractFileData(string packfilePath, string outputPath, Stream stream)
        {
            Directory.CreateDirectory(outputPath);
            if (Header.Compressed && Header.Condensed)
            {
                DeserializeCompressedAndCondensed(packfilePath, outputPath, stream);
            }
            else
            {
                if (Header.Compressed)
                {
                    DeserializeCompressed(packfilePath, outputPath, stream);
                }
                else
                {
                    DeserializeDefault(packfilePath, outputPath, stream);
                }
            }
        }

        private void DeserializeCompressedAndCondensed(string packfilePath, string outputPath, Stream stream)
        {
            var packfile = new BinaryReader(stream);
            string packfileName = Path.GetFileName(packfilePath);
            //Inflate whole data block
            byte[] compressedData = new byte[Header.CompressedDataSize];
            byte[] decompressedData = new byte[Header.DataSize];
            packfile.Read(compressedData, 0, (int)Header.CompressedDataSize);

            int decompressedSizeResult = 0;
            using (MemoryStream memory = new MemoryStream(compressedData))
            {
                using (InflaterInputStream inflater = new InflaterInputStream(memory))
                {
                    decompressedSizeResult = inflater.Read(decompressedData, 0, (int)Header.DataSize);
                }
            }
            if (decompressedSizeResult != Header.DataSize)
            {
                var errorString = new StringBuilder();
                errorString.AppendFormat(
                    "Error while deflating {0}! Decompressed data size is {1} bytes, while" +
                    " it should be {2} bytes according to header data.", packfileName,
                    decompressedSizeResult, Header.DataSize);
                Console.WriteLine(errorString.ToString());
                throw new Exception(errorString.ToString());
            }

            foreach (var entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
            {
                long decompressedPosition = entry.Value.DataOffset;
                if (Verbose)
                {
                    Console.Write("{0}> Extracting {1}...", packfileName, Filenames[entry.Index]);
                }
                var writer = new BinaryWriter(System.IO.File.Create(outputPath + Filenames[entry.Index]));

                for (long i = 0; i < entry.Value.DataSize; i++)
                {
                    writer.Write(decompressedData[decompressedPosition + i]);
                }
                if (Verbose)
                {
                    Console.WriteLine(" Done!");
                }
            }
        }

        private void DeserializeCompressed(string packfilePath, string outputPath, Stream stream)
        {
            var packfile = new BinaryReader(stream);
            string packfileName = Path.GetFileName(packfilePath);
            //Inflate block by block
            foreach (var Entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
            {
                if (Verbose)
                {
                    Console.Write("{0}> Extracting {1}...", packfileName, Filenames[Entry.Index]);
                }
                byte[] compressedData = new byte[Entry.Value.CompressedDataSize];
                byte[] decompressedData = new byte[Entry.Value.DataSize];
                packfile.Read(compressedData, 0, (int)Entry.Value.CompressedDataSize);

                int decompressedSizeResult = 0;
                using (var memory = new MemoryStream(compressedData))
                {
                    using (InflaterInputStream inflater = new InflaterInputStream(memory))
                    {
                        decompressedSizeResult = inflater.Read(decompressedData, 0, (int)Entry.Value.DataSize);
                    }
                }
                if (decompressedSizeResult != Entry.Value.DataSize)
                {
                    var errorString = new StringBuilder();
                    errorString.AppendFormat(
                        "Error while deflating {0} in {1}! Decompressed data size is {2} bytes, while" +
                        " it should be {3} bytes according to header data.", Filenames[Entry.Index],
                        packfileName, decompressedSizeResult, Entry.Value.DataSize);
                    Console.WriteLine(errorString.ToString());
                    throw new Exception(errorString.ToString());
                }
                File.WriteAllBytes(outputPath + Filenames[Entry.Index], decompressedData);

                int remainder = (int)(packfile.BaseStream.Position % 2048);
                if (remainder > 0)
                {
                    packfile.ReadBytes(2048 - remainder); //Alignment Padding
                }
                if (Verbose)
                {
                    Console.WriteLine(" Done!");
                }
            }
        }

        private void DeserializeDefault(string packfilePath, string outputPath, Stream stream)
        {
            var packfile = new BinaryReader(stream);
            string packfileName = Path.GetFileName(packfilePath);
            //Copy data into individual files
            foreach (var Entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
            {
                if (Verbose)
                {
                    Console.Write("{0}> Extracting {1}...", packfileName, Filenames[Entry.Index]);
                }
                byte[] fileData = new byte[Entry.Value.DataSize];
                packfile.Read(fileData, 0, (int)Entry.Value.DataSize);
                File.WriteAllBytes(outputPath + Filenames[Entry.Index], fileData);

                if (!Header.Condensed)
                {
                    //If you remove the parentheses here you'll break unpacking on terr01_l0.vpp_pc

                    int remainder = (int)(packfile.BaseStream.Position % 2048);
                    if (remainder > 0)
                    {
                        packfile.ReadBytes(2048 - remainder); //Alignment Padding
                    }
                }
                if (Verbose)
                {
                    Console.WriteLine(" Done!");
                }
            }
        }

        public void WriteToBinary(string inputPath, string outputPath, bool compressed = false, bool condensed = false)
        {
            if (!Directory.Exists(inputPath))
            {
                Console.WriteLine("The input path provided is not a directory or does not exist. Cannot pack!");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            //if (!File.Exists(outputPath))
            //{
            //    Console.WriteLine("The output path provided is not a file. Cannot pack!");
            //    return;
            //}

            Filenames = new List<string>();
            DirectoryEntries = new List<PackfileEntry>();

            //Read input folder and generate header/file data
            uint currentNameOffset = 0;
            uint currentDataOffset = 0;
            uint totalNamesSize = 0; //Tracks size of uncompressed data
            uint totalDataSize = 0; //Tracks size of file names
            var inputFolder = new DirectoryInfo(inputPath).GetFiles();
            foreach (var file in inputFolder)
            {
                Filenames.Add(file.Name);
                DirectoryEntries.Add(new PackfileEntry
                {
                    CompressedDataSize = compressed ? 0 : 0xFFFFFFFF, //Not known, set to 0xFFFFFFFF if not compressed //Double check what this should be when C&C 
                    DataOffset = currentDataOffset,
                    NameHash = HashVolitionString(file.Name),
                    DataSize = (uint)file.Length,
                    NameOffset = currentNameOffset,
                    PackagePointer = 0, //always zero
                    Sector = 0, //always zero
                    FullPath = file.FullName
                });

                currentNameOffset += (uint)file.Length + 1;
                if (!compressed) //If compressed then the data offset is calc'd during compression
                {
                    currentDataOffset += (uint)file.Length;
                    if (!condensed)
                    {
                        currentDataOffset += GetAlignmentPad(currentDataOffset);
                    }
                }

                totalDataSize += (uint)file.Length;
                totalNamesSize += (uint)file.Name.Length + 1;
            }

            uint packfileFlags = 0;
            if (compressed)
            {
                packfileFlags &= 1;
            }
            if (condensed)
            {
                packfileFlags &= 2;
            }

            Header = new PackfileHeader()
            {
                Signature = 0x51890ACE,
                Version = 3,
                ShortName = new char[65],
                PathName = new char[256],
                Flags = packfileFlags,
                NumberOfFiles = (uint)inputFolder.Length,
                FileSize = 0, //Not yet known
                DirectoryBlockSize = 7 * 4 * (uint)inputFolder.Length,
                FilenameBlockSize = totalNamesSize,
                DataSize = totalDataSize, //Double check that this doesn't count padding
                CompressedDataSize = compressed ? 0 : 0xFFFFFFFF, //Not known, set to 0xFFFFFFFF if not compressed
            };

            //Write header, directory block, and names block to disk
            File.Delete(outputPath);
            var writer = new BinaryWriter(new FileStream(outputPath, FileMode.Create));

            Header.WriteToBinary(writer);

            foreach (var entry in DirectoryEntries)
            {
                entry.WriteToBinary(writer);
            }

            int padding1 = GetAlignmentPad(writer.BaseStream.Position);
            writer.Write(Enumerable.Repeat((byte)0x0, padding1).ToArray(), 0, padding1);
            //writer.Write(new Byte []{0x0}, 0, GetAlignmentPad(writer.BaseStream.Position));

            foreach (var filename in Filenames)
            {
                writer.Write(filename);
                writer.Write(new byte[] { 0x0 });
            }

            int padding2 = GetAlignmentPad(writer.BaseStream.Position);
            writer.Write(Enumerable.Repeat((byte)0x0, padding2).ToArray(), 0, padding2);

            //Start compressing shit and writing it to the disk

            if (compressed && condensed)
            {
                //Compress entire data section as one block
                var uncompressedDataBlock = new List<byte>();

                foreach (var entry in DirectoryEntries)
                {
                    byte[] subFileData = File.ReadAllBytes(entry.FullPath);
                    Header.DataSize += (uint)subFileData.Length;
                    uncompressedDataBlock.AddRange(subFileData);
                }

                int compressedSizeResult = 0;
                byte[] compressedData = { };
                using (MemoryStream memory = new MemoryStream(uncompressedDataBlock.ToArray()))
                {
                    using (var deflater = new DeflaterOutputStream(memory))
                    {
                        compressedSizeResult = deflater.Read(compressedData, 0, Int32.MaxValue);
                    }
                }

                Header.CompressedDataSize = (uint)compressedSizeResult; //Need to update this in the file after
                writer.Write(compressedData);
                //todo: remember to update data size as well
            }
            else
            {
                if (compressed)
                {
                    //Compress each file separately with padding
                    foreach (var entry in DirectoryEntries)
                    {
                        byte[] subFileData = File.ReadAllBytes(entry.FullPath);
                        Header.DataSize += (uint)subFileData.Length;

                        int compressedSizeResult = 0;
                        byte[] compressedData = { };
                        using (MemoryStream memory = new MemoryStream(subFileData))
                        {
                            //using (var deflater = new DeflaterOutputStream(memory))
                            using (var deflater = new DeflateStream(memory, CompressionLevel.Optimal))
                            {
                                //compressedSizeResult = deflater.Read(compressedData, 0, Int32.MaxValue);
                                compressedData = new byte[deflater.Length];
                                //deflater.
                                //var defl = new DeflateStream(memory, CompressionLevel.Optimal);
                                //deflater.Write(compressedData, 0, (int)deflater.Length);
                            }
                        }

                        writer.Write(compressedData);
                        int paddingSize = GetAlignmentPad(writer.BaseStream.Position);
                        writer.Write(Enumerable.Repeat((byte)0x0, paddingSize).ToArray(), 0, paddingSize);

                        Header.CompressedDataSize += (uint)compressedSizeResult + (uint)paddingSize;
                        //todo: remember to update data size as well
                    }
                }
                else
                {
                    //No compression, pad data if not condensed
                    foreach (var entry in DirectoryEntries)
                    {
                        byte[] subFileData = File.ReadAllBytes(entry.FullPath);
                        writer.Write(subFileData);
                        Header.DataSize += (uint)subFileData.Length;
                        if (!condensed)
                        {
                            int paddingSize = GetAlignmentPad(writer.BaseStream.Position);
                            writer.Write(Enumerable.Repeat((byte)0x0, paddingSize).ToArray(), 0, paddingSize);
                            Header.DataSize += (uint)paddingSize;
                        }
                    }
                }
            }

            //Go back and fill in any previously unknown info like compressed data size and total data size
            writer.Seek(344, SeekOrigin.Begin); //Seek to FileSize
            writer.Write(Header.FileSize);
        }

        // Full credit for this function goes to gibbed. This is used to generate
        // the filename hashes while packing packfiles. Link to this function in
        // his code: https://github.com/gibbed/Gibbed.Volition/blob/d2da5c26ccf1d09726ff4c58b81ae709b89b8db5/projects/Gibbed.Volition.FileFormats/StringHelpers.cs#L68
        public static uint HashVolitionString(string input)
        {
            input = input.ToLowerInvariant();

            uint hash = 0;
            for (int i = 0; i < input.Length; i++)
            {
                // rotate left by 6
                hash = (hash << 6) | (hash >> (32 - 6));
                hash = (char)(input[i]) ^ hash;
            }
            return hash;
        }

        int GetAlignmentPad(int position)
        {
            int remainder = position % 2048;
            if (remainder > 0)
            {
                return 2048 - remainder;
            }
            return 0;
        }

        int GetAlignmentPad(long position)
        {
            int remainder = (int)(position % 2048);
            if (remainder > 0)
            {
                return 2048 - remainder;
            }
            return 0;
        }

        uint GetAlignmentPad(uint position)
        {
            uint remainder = position % 2048;
            return 2048 - remainder;
        }
    }
}
