using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using OGE.Helpers;
using RfgTools.Formats.Asm;

namespace RfgTools.Formats.Packfiles
{
    public class Packfile
    {
        //Header block
        public PackfileHeader Header;

        //Directory block
        public List<PackfileEntry> DirectoryEntries = new List<PackfileEntry>();

        //Filenames block
        public List<string> Filenames = new List<string>();


        //Data below here is only used by tools, not actually in packfiles
        public bool Verbose = false;

        public uint DataStartOffset = 0;
        public string Filename { get; private set; }
        public string PackfilePath { get; private set; }
        public bool MetadataWasRead { get; private set; } = false;

        //Table of content files in this packfile, has info about contents of str2_pc files.
        public List<AsmFile> AsmFiles = new List<AsmFile>();
        public bool ContainsAsmFiles { get; private set; } = false;

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
            PackfilePath = packfilePath;
            string packfileName = Path.GetFileName(packfilePath);
            var packfileInfo = new FileInfo(packfilePath);
            Filename = packfileName;

            Console.WriteLine("Extracting " + packfileName + "...");
            if (packfileInfo.Length <= 2048)
            {
                Console.WriteLine($"Cancelled extraction of {packfileName}. Packfile is empty!");
                return;
            }
            if (Verbose)
            {
                Console.WriteLine(packfileName + "> Reading header data...");
            }

            var packfile = new BinaryReader(new FileStream(packfilePath, FileMode.Open));
            Header = new PackfileHeader();
            Header.ReadFromBinary(packfile);

            for (int i = 0; i < Header.NumberOfFiles; i++)
            {
                var entry = new PackfileEntry();
                entry.ReadFromBinary(packfile);
                DirectoryEntries.Add(entry);
            }
            packfile.ReadBytes(2048 - ((int)packfile.BaseStream.Position % 2048)); //Alignment Padding

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

                if (Path.GetExtension(name.ToString()) == ".asm_pc")
                    ContainsAsmFiles = true;
                DirectoryEntries[i].FileName = name.ToString();
                DirectoryEntries[i].Extension = Path.GetExtension(name.ToString());
            }
            packfile.ReadBytes(2048 - ((int)packfile.BaseStream.Position % 2048)); //Alignment Padding
            DataStartOffset = (uint)packfile.BaseStream.Position;
            MetadataWasRead = true;
            packfile.Dispose();

            //Fix data offsets. Values in packfile not always valid.
            FixEntryDataOffsets();
        }

        /// <summary>
        /// Fix data offsets. Values in packfile not always valid.
        /// Ignores packfiles that are compressed AND condensed since those must
        /// be fully extracted and data offsets aren't relevant in that case.
        /// </summary>
        private void FixEntryDataOffsets()
        {
            if (Header.Compressed && Header.Condensed) 
                return;

            long runningDataOffset = 0; //Track relative offset from data section start
            foreach (var entry in DirectoryEntries)
            {
                //Set entry offset
                entry.DataOffset = runningDataOffset;

                //Update offset based on entry size and storage type
                if (Header.Compressed) //Compressed, not condensed
                {
                    if (runningDataOffset + entry.CompressedDataSize > uint.MaxValue)
                    {
                        runningDataOffset += entry.CompressedDataSize;
                    }
                    else
                    {
                        runningDataOffset += entry.CompressedDataSize;
                    }

                    long alignmentPad = GetAlignmentPad(runningDataOffset);
                    if (runningDataOffset + alignmentPad > uint.MaxValue)
                    {
                        runningDataOffset += alignmentPad;
                    }
                    else
                    {
                        runningDataOffset += alignmentPad;
                    }
                }
                else //Not compressed, maybe condensed
                {
                    if (runningDataOffset + entry.DataSize > uint.MaxValue)
                    {
                        runningDataOffset += entry.DataSize;
                    }
                    else
                    {
                        runningDataOffset += entry.DataSize;
                    }

                    if (!Header.Condensed)
                    {
                        long alignmentPad = GetAlignmentPad(runningDataOffset);
                        if (runningDataOffset + alignmentPad > uint.MaxValue)
                        {
                            runningDataOffset += GetAlignmentPad(runningDataOffset);
                        }
                        else
                        {
                            runningDataOffset += GetAlignmentPad(runningDataOffset);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parses the asm_pc files in the packfile, and stores data about them in AsmFiles.
        /// Allows viewing of the files in str2_pc files in vpp_pc files, without extracting them.
        /// </summary>
        /// <param name="outputPath">Folder to extract the asm_pc file(s) to.</param>
        public void ParseAsmFiles(string outputPath)
        {
            if (!MetadataWasRead || !ContainsAsmFiles)
                return;

            if (Path.GetFileName(PackfilePath) == "terr01_l0.vpp_pc")
            {
                var a = 2;
            }

            Directory.CreateDirectory(outputPath);
            foreach (var entry in DirectoryEntries)
            {
                if (entry.Extension != ".asm_pc")
                    continue;
                if (!TryExtractSingleFile(entry.FileName, $"{outputPath}{entry.FileName}"))
                    ExtractFileData(outputPath);

                var asmFile = new AsmFile();
                asmFile.ReadFromBinary($"{outputPath}{entry.FileName}");
                AsmFiles.Add(asmFile);
            }
        }

        public bool CanExtractSingleFile()
        {
            return Header != null && !(Header.Compressed && Header.Condensed);
        }

        public bool TryExtractSingleFile(string subFileName, string outputPath)
        {
            if (!CanExtractSingleFile())
                return false;
            if (!Filenames.Contains(subFileName) || !MetadataWasRead)
                return false;

            int subFileIndex = Filenames.IndexOf(subFileName);
            if (subFileIndex == -1)
                return false;

            var entry = DirectoryEntries[subFileIndex];
            var packfile = GetStreamAtDataSectionStart();
            if (packfile == Stream.Null)
                return false;

            packfile.Seek(entry.DataOffset, SeekOrigin.Current);
            if (Header.Compressed)
            {
                var bytes = new byte[entry.CompressedDataSize];
                packfile.Read(bytes, 0, (int)entry.CompressedDataSize);
                if (!CompressionHelpers.TryZlibInflate(bytes, entry.DataSize, out byte[] decompressedData, out _))
                    return false;
                File.WriteAllBytes(outputPath, decompressedData);
            }
            else
            {
                if (subFileName == "terr01_l0.asm_pc")
                {
                    var b = 2;
                }
                var bytes = new byte[entry.DataSize];
                packfile.Read(bytes, 0, (int)entry.DataSize);
                File.WriteAllBytes(outputPath, bytes);
            }
            return true;
        }

        private Stream GetStreamAtDataSectionStart()
        {
            if (!MetadataWasRead)
                return Stream.Null;

            var stream = new FileStream(PackfilePath, FileMode.Open, FileAccess.Read);
            stream.Seek(DataStartOffset, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// Extracts subfiles from the packfile and places them in outputPath,
        /// expects that ReadMetadata() was already called.
        /// </summary>
        /// <param name="outputPath"></param>
        public void ExtractFileData(string outputPath)
        {
            if (!MetadataWasRead)
                return;

            Stream stream = GetStreamAtDataSectionStart();

            Filename = Path.GetFileName(PackfilePath);
            Directory.CreateDirectory(outputPath);
            if (Header.Compressed && Header.Condensed)
            {
                DeserializeCompressedAndCondensed(PackfilePath, outputPath, stream);
            }
            else
            {
                if (Header.Compressed)
                {
                    DeserializeCompressed(PackfilePath, outputPath, stream);
                }
                else
                {
                    DeserializeDefault(PackfilePath, outputPath, stream);
                }
            }
            stream.Dispose();
        }

        private void DeserializeCompressedAndCondensed(string packfilePath, string outputPath, Stream stream)
        {
            PackfilePath = packfilePath;
            var packfile = new BinaryReader(stream);
            string packfileName = Path.GetFileName(packfilePath);

            //Copy whole data block to buffer and inflate it.
            byte[] compressedData = new byte[Header.CompressedDataSize];
            packfile.Read(compressedData, 0, (int)Header.CompressedDataSize);

            if (!CompressionHelpers.TryZlibInflate(compressedData, Header.DataSize, out byte[] decompressedData, out int decompressedSizeResult))
            {
                string errorString = $"Error while deflating {packfileName}! Decompressed data size is {decompressedSizeResult} " +
                                     $"bytes, while it should be {Header.DataSize} bytes according to header data.";
                Console.WriteLine(errorString);
                throw new Exception(errorString);
            }

            for (int i = 0; i < DirectoryEntries.Count; i++)
            {
                var entry = DirectoryEntries[i];
                long decompressedPosition = entry.DataOffset;
                if (Verbose)
                    Console.Write("{0}> Extracting {1}...", packfileName, Filenames[i]);

                using (var writer = new BinaryWriter(System.IO.File.Create(outputPath + Filenames[i])))
                {
                    for (long j = 0; j < entry.DataSize; j++)
                    {
                        writer.Write(decompressedData[decompressedPosition + j]);
                    }
                }
                if (Verbose)
                    Console.WriteLine(" Done!");
            }
        }

        private void DeserializeCompressed(string packfilePath, string outputPath, Stream stream)
        {
            PackfilePath = packfilePath;
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

                //Todo: Switch to use CompressionHelpers.TryZlibDeflate()
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
            PackfilePath = packfilePath;
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

        //Todo: Finish this function and test that it works on all vpps & str2s
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

        //Todo: Move these into relevant helpers/namespaces
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
            if (remainder > 0)
            {
                return 2048 - remainder;
            }
            return 0;
        }
    }
}
