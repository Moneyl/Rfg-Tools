using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using OGE.Helpers;
using RfgTools.Formats.Asm;
using RfgTools.Helpers;
using RfgTools.Helpers.Gibbed.IO;
using RfgTools.Helpers.Gibbed.Volition.FileFormats;

namespace RfgTools.Formats.Packfiles
{
    public class Packfile
    {
        //Header block
        public PackfileHeader Header;

        //Directory block
        public List<PackfileEntry> DirectoryEntries = new List<PackfileEntry>();

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

            using var stream = new FileStream(packfilePath, FileMode.Open);
            var packfile = new BinaryReader(stream);
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

                    long alignmentPad = BinaryHelpers.GetAlignmentPad(runningDataOffset);
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
                        long alignmentPad = BinaryHelpers.GetAlignmentPad(runningDataOffset);
                        if (runningDataOffset + alignmentPad > uint.MaxValue)
                        {
                            runningDataOffset += BinaryHelpers.GetAlignmentPad(runningDataOffset);
                        }
                        else
                        {
                            runningDataOffset += BinaryHelpers.GetAlignmentPad(runningDataOffset);
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

        public bool SubFileExists(string subFileName)
        {
            foreach (var entry in DirectoryEntries)
            {
                if (entry.FileName == subFileName)
                    return true;
            }
            return false;
        }

        public int GetSubfileIndex(string subFileName)
        {
            for (var i = 0; i < DirectoryEntries.Count; i++)
            {
                if (DirectoryEntries[i].FileName == subFileName)
                    return i;
            }
            return -1;
        }

        public bool TryGetSubfileEntry(string subFileName, out PackfileEntry entry)
        {
            entry = null;
            int index = GetSubfileIndex(subFileName);
            if (index < 0)
                return false;

            entry = DirectoryEntries[index];
            return true;
        }

        public bool TryExtractSingleFile(string subFileName, string outputPath)
        {
            if (!CanExtractSingleFile())
                return false;
            if (!SubFileExists(subFileName) || !MetadataWasRead)
                return false;

            int subFileIndex = GetSubfileIndex(subFileName);
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
                ReadDataCompressedAndCondensed(PackfilePath, outputPath, stream);
            }
            else
            {
                if (Header.Compressed)
                {
                    ReadDataCompressed(PackfilePath, outputPath, stream);
                }
                else
                {
                    ReadDataDefault(PackfilePath, outputPath, stream);
                }
            }
            stream.Dispose();

            if(Path.GetExtension(Filename) == ".str2_pc")
                WriteStreamsFile(Path.GetDirectoryName(outputPath));
        }

        private void ReadDataCompressedAndCondensed(string packfilePath, string outputPath, Stream stream)
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
                    Console.Write("{0}> Extracting {1}...", packfileName, entry.FileName);

                using var writer = new BinaryWriter(System.IO.File.Create(outputPath + entry.FileName));
                for (long j = 0; j < entry.DataSize; j++)
                {
                    writer.Write(decompressedData[decompressedPosition + j]);
                }

                if (Verbose)
                    Console.WriteLine(" Done!");
            }
        }

        private void ReadDataCompressed(string packfilePath, string outputPath, Stream stream)
        {
            PackfilePath = packfilePath;
            var packfile = new BinaryReader(stream);
            string packfileName = Path.GetFileName(packfilePath);
            //Inflate block by block
            foreach (var Entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
            {
                if (Verbose)
                {
                    Console.Write("{0}> Extracting {1}...", packfileName, Entry.Value.FileName);
                }
                byte[] compressedData = new byte[Entry.Value.CompressedDataSize];
                byte[] decompressedData = new byte[Entry.Value.DataSize];
                packfile.Read(compressedData, 0, (int)Entry.Value.CompressedDataSize);

                //Todo: Switch to use CompressionHelpers.TryZlibDeflate()
                //int decompressedSizeResult = 0;
                //using var memory = new MemoryStream(compressedData);
                //using InflaterInputStream inflater = new InflaterInputStream(memory);
                //decompressedSizeResult = inflater.Read(decompressedData, 0, (int)Entry.Value.DataSize);

                if (!CompressionHelpers.TryZlibInflate(compressedData, (uint)decompressedData.Length, out decompressedData, out int decompressedSizeResult))
                {
                    throw new Exception($"Error while deflating {Entry.Value.FileName} in {packfileName}! Failed to inflate!");
                }

                if (decompressedSizeResult != Entry.Value.DataSize)
                {
                    var errorString = new StringBuilder();
                    errorString.Append(
                        $"Error while deflating {Entry.Value.FileName} in {packfileName}! " +
                        $"Decompressed data size is {decompressedSizeResult} bytes, while " +
                        $"it should be {Entry.Value.DataSize} bytes according to header data.");
                    Console.WriteLine(errorString.ToString());
                    throw new Exception(errorString.ToString());
                }
                File.WriteAllBytes(outputPath + Entry.Value.FileName, decompressedData);

                packfile.Align();
                //int remainder = (int)(packfile.BaseStream.Position % 2048);
                //int padding = GetAlignmentPad(packfile.BaseStream.Position);
                //if (padding > 0)
                //{
                //    packfile.Skip(padding); //Alignment Padding
                //}
                if (Verbose)
                {
                    Console.WriteLine(" Done!");
                }
            }
        }

        private void ReadDataDefault(string packfilePath, string outputPath, Stream stream)
        {
            PackfilePath = packfilePath;
            var packfile = new BinaryReader(stream);
            string packfileName = Path.GetFileName(packfilePath);
            //Copy data into individual files
            foreach (var Entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
            {
                if (Verbose)
                {
                    Console.Write("{0}> Extracting {1}...", packfileName, Entry.Value.FileName);
                }
                byte[] fileData = new byte[Entry.Value.DataSize];
                packfile.Read(fileData, 0, (int)Entry.Value.DataSize);
                File.WriteAllBytes(outputPath + Entry.Value.FileName, fileData);

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

        /// <summary>
        /// Writes an xml file containing a list of files stored in the packfile.
        /// Listed in the order that they are stored in the packfile. This is used
        /// to ensure that str2_pc files and unpacked and repacked with their
        /// primitives in the same order. If they are not, this will break the game
        /// as it expects them to be in the order listed in the asm_pc files.
        /// This uses the same naming convention as gibbeds unpacker,
        /// a file called @streams.xml, for compatibility.
        /// </summary>
        /// <param name="outputFolderPath">Path of of the folder to write the streams file to.</param>
        public void WriteStreamsFile(string outputFolderPath)
        {
            //Xml file representation in memory
            var xml = new XDocument();

            //Add root node.
            var root = new XElement("streams", 
                new XAttribute("endian", "Little"),
                new XAttribute("compressed", Header.Compressed.ToString()),
                new XAttribute("condensed", Header.Condensed.ToString()));
            xml.Add(root);

            //Write entry names
            foreach (var entry in DirectoryEntries)
            {
                var entryNode = new XElement("entry", entry.FileName, new XAttribute("name", entry.FileName));
                root.Add(entryNode);
            }

            using var stream = new FileStream($"{outputFolderPath}\\@streams.xml", FileMode.OpenOrCreate);
            xml.Save(stream);
        }

        /// <summary>
        /// Read @streams.xml file written by <see cref="WriteStreamsFile"/>
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <returns>Returns tuple with data of (bool compressed, bool condensed, List<string> EntryNames</returns>
        public (bool, bool, List<string>) ReadStreamsFile(string inputFilePath)
        {
            using var settingsFileStream = new FileStream(inputFilePath, FileMode.Open);
            var document = XDocument.Load(settingsFileStream);

            var root = document.Root;
            if (root == null)
                throw new XmlException("Error! @streams.xml file has no root node!");

            bool compressed = root.GetRequiredAttributeValue("compressed").ToBool();
            bool condensed = root.GetRequiredAttributeValue("condensed").ToBool();
            var entryNames = new List<string>();

            foreach (var entry in root.Elements("entry"))
            {
                entryNames.Add(entry.GetRequiredAttributeValue("name"));
            }

            return (compressed, condensed, entryNames);
        }

        /// <summary>
        /// Reads all files in inputPath, and packs them into a packfile saved to outputPath.
        /// </summary>
        /// <param name="inputPath">Path of the folder containing the files that should be packed.</param>
        /// <param name="outputPath">Path and name to save the packfile to.</param>
        /// <param name="useStreamsFile">If true expects to find a file called @streams.xml in the
        /// inputPath folder. If true overrides the compressed and condensed arguments. Used to
        /// ensure that str2_pc files are packed with files in the order that the game expects.
        /// </param>
        /// <param name="compressed">Whether or not to compress the file data. Ignored if useStreamsFile is true.</param>
        /// <param name="condensed">Whether or not to condense the file data. Ignored if useStreamsFile is true.</param>
        public void WriteToBinary(string inputPath, string outputPath, bool useStreamsFile = false, bool compressed = false, bool condensed = false)
        {
            if (!Directory.Exists(inputPath))
            {
                Console.WriteLine("The input path provided is not a directory or does not exist. Cannot pack!");
                return;
            }

            DirectoryEntries = new List<PackfileEntry>();
            var streamFileEntries = new List<string>();
            if (useStreamsFile)
                (compressed, condensed, streamFileEntries) = ReadStreamsFile($"{inputPath}\\@streams.xml");

            //Read input folder and generate header/file data
            uint currentNameOffset = 0;
            uint currentDataOffset = 0;
            uint totalNamesSize = 0; //Tracks size of uncompressed data
            uint totalDataSize = 0; //Tracks size of file names
            var inputFiles = new DirectoryInfo(inputPath).GetFiles();

            if (useStreamsFile)
            {
                //Reorder files in order of streamFileEntries. Ensures order matches asm_pc order.
                var temporaryEntriesList = new List<FileInfo>();
                foreach (var entryName in streamFileEntries)
                {
                    FileInfo targetEntry = null;
                    foreach (var entry in inputFiles)
                    {
                        if (entry.Name == entryName)
                            targetEntry = entry;
                    }
                    temporaryEntriesList.Add(targetEntry);
                }
                inputFiles = temporaryEntriesList.ToArray();
            }

            using var stream = new FileStream(outputPath, FileMode.Create);
            using var writer = new BinaryWriter(stream);

            foreach (var file in inputFiles)
            {
                if(file.Name == "@streams.xml")
                    continue;

                DirectoryEntries.Add(new PackfileEntry
                {
                    CompressedDataSize = compressed ? 0 : 0xFFFFFFFF, //Not known, set to 0xFFFFFFFF if not compressed //Double check what this should be when C&C 
                    DataOffset = currentDataOffset,
                    NameHash = file.Name.HashVolition(),
                    DataSize = (uint)file.Length,
                    NameOffset = currentNameOffset,
                    PackagePointer = 0, //always zero
                    Sector = 0, //always zero
                    FullPath = file.FullName,
                    FileName = file.Name
                });

                //Update name and data offsets
                currentNameOffset += (uint)file.Name.Length + 1;
                currentDataOffset += (uint)file.Length;
                if (!condensed)
                    currentDataOffset += writer.GetAlignmentPad(currentDataOffset);

                totalDataSize += (uint)file.Length;
                totalNamesSize += (uint)file.Name.Length + 1;
            }

            uint packfileFlags = 0;
            if (compressed) 
                packfileFlags |= 1;
            if (condensed) 
                packfileFlags |= 2;

            Header = new PackfileHeader
            {
                Signature = 0x51890ACE,
                Version = 3,
                ShortName = new char[65],
                PathName = new char[256],
                Flags = packfileFlags,
                NumberOfFiles = (uint)DirectoryEntries.Count,
                FileSize = 0, //Not yet known, set after writing file data
                DirectoryBlockSize = (uint)DirectoryEntries.Count * 28, //Todo: Include padding?
                FilenameBlockSize = totalNamesSize,
                DataSize = totalDataSize, //Todo: Double check that this doesn't count padding
                CompressedDataSize = compressed ? 0 : 0xFFFFFFFF, //Not known, set to 0xFFFFFFFF if not compressed
            };

            //Skip to data section offset. We write the file data first, then come back
            //and write the header, entries, and file names
            int dataOffset = GuessDataOffset();
            writer.Skip(dataOffset);

            //Write subfile data
            if(compressed && condensed)
                WriteDataCompressedAndCondensed(writer.BaseStream);
            else if(compressed)
                WriteDataCompressed(writer.BaseStream);
            else
                WriteDataDefault(writer, condensed);

            //Write header
            Header.FileSize = (uint)writer.BaseStream.Length;
            writer.Seek(0, SeekOrigin.Begin);
            Header.WriteToBinary(writer);

            //Write entries and names
            foreach (var entry in DirectoryEntries)
            {
                entry.WriteToBinary(writer);
            }
            writer.Align();

            foreach (var entry in DirectoryEntries)
            {
                writer.WriteNullTerminatedString(entry.FileName);
            }
            writer.Align();
        }

        //Todo: Fix this case. Maybe try switching zlib library used to the one used by gibbed's tools
        private void WriteDataCompressedAndCondensed(Stream stream)
        {
            long lastPos = stream.Position;

            //NOTE: Change this if compression level changes
            //Write zlib headers because DeflateStream doesn't do this for some reason... 
            stream.Write(new byte[] { 0x78, 0xDA });

            using (var deflateStream = new DeflateStream(stream, CompressionLevel.Optimal, true))
            {
                //Compress each subfile
                foreach (var entry in DirectoryEntries)
                {
                    byte[] uncompressedData = File.ReadAllBytes(entry.FullPath);
                    deflateStream.Write(uncompressedData);
                    deflateStream.Flush();

                    entry.CompressedDataSize = (uint)(stream.Position - lastPos);
                    lastPos = stream.Position;
                    Header.CompressedDataSize += entry.CompressedDataSize;
                }
            }

            //Manually add header bytes size
            DirectoryEntries[0].CompressedDataSize += 2;
            Header.CompressedDataSize += 2;

            //DeflateStream writes 2 bytes of junk once it's disposed. No idea why, but this is to fix that.
            if (stream.Position != lastPos)
                stream.SetLength(lastPos);
        }

        private void WriteDataCompressed(Stream stream)
        {
            long lastPos = stream.Position;
            using var writer = new BinaryWriter(stream, Encoding.Default, true);

            //Compress each subfile
            foreach (var entry in DirectoryEntries)
            {
                using (var deflateStream = new DeflateStream(stream, CompressionLevel.Optimal, true))
                {
                    //NOTE: Change this if compression level changes
                    //Write zlib headers because DeflateStream doesn't do this for some reason... 
                    stream.Write(new byte[] { 0x78, 0xDA });

                    //Compress data from each subfile and write to file
                    byte[] uncompressedData = File.ReadAllBytes(entry.FullPath);
                    deflateStream.Write(uncompressedData);
                    deflateStream.Flush();

                    //Write padding bytes and track size (padding doesn't count for total compressed data size)
                    entry.CompressedDataSize = (uint)(stream.Position - lastPos); //Don't count padding
                    Header.CompressedDataSize += entry.CompressedDataSize;
                    
                    writer.Align();
                    lastPos = stream.Position;
                }
                //DeflateStream writes 2 bytes of junk once it's disposed. No idea why, but this is to fix that.
                if (stream.Position != lastPos)
                    stream.Seek(lastPos, SeekOrigin.Begin);
            }
        }

        private void WriteDataDefault(BinaryWriter writer, bool condensed)
        {
            //No compression, pad data if not condensed
            foreach (var entry in DirectoryEntries)
            {
                byte[] subFileData = File.ReadAllBytes(entry.FullPath);
                writer.Write(subFileData);

                if (!condensed) 
                    Header.DataSize += (uint) writer.Align();
            }
        }

        //From Gibbed.Volition repo: https://github.com/gibbed/Gibbed.Volition
        public int GuessDataOffset()
        {
            int totalSize = 0;
            totalSize += 2048; //Header size
            totalSize += (DirectoryEntries.Count * 28).Align(2048); //Entries size
            totalSize += DirectoryEntries.Sum(entry => entry.FileName.Length + 1).Align(2048); //File names size
            return totalSize;
        }
    }
}
