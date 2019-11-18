#define IgnoreRelationData

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using RfgTools.Helpers;

namespace RfgTools.Formats.Zones
{
    public class ZonePc //.rfgzone_pc & layer_pc format
    {
        public uint Signature;
        public uint Version;
        public uint NumObjects;
        public uint NumHandles;
        public uint DistrictHash;
        public uint DistrictFlags;

        public List<ZoneObject> Objects = new List<ZoneObject>();

        /// <summary>
        /// Attempt to read the rfgzone_pc/layer_pc format from the file at this path. Handles making the stream.
        /// </summary>
        /// <param name="inputPath">Path of the file to read the zone data from.</param>
        public void ReadFromBinary(string inputPath)
        {
            ReadFromBinary(new FileStream(inputPath, FileMode.Open));
        }

        /// <summary>
        /// Attempt to read the rfgzone_pc/layer_pc format from the stream. Fill this class with that data.
        /// </summary>
        /// <param name="stream">The stream to read the data from.</param>
        public void ReadFromBinary(Stream stream)
        {
            var reader = new BinaryReader(stream);
            
            //Read header
            Signature = reader.ReadUInt32();
            if (Signature != 1162760026) //Should equal ascii value for "ZONE"
            {
                Console.WriteLine($"Error! Invalid file signature. Expected 1162760026, detected {Signature}. Make sure the vpp/str2 containing this file was extracted properly. Exiting.");
                return;
            }
            Version = reader.ReadUInt32();
            if (Version != 36) //Only have seen and reversed version 36
            {
                Console.WriteLine($"Error! Unsupported format version. Expected 36, detected {Version}. Exiting.");
                return;
            }
            NumObjects = reader.ReadUInt32();
            NumHandles = reader.ReadUInt32();
            DistrictHash = reader.ReadUInt32(); //Todo: Figure out how district hashes are generated
            DistrictFlags = reader.ReadUInt32(); 

            //Todo: Make sure it's safe to skip this on zones with numHandles != 0
            //Todo: Try deleting relational data on all zones and seeing if that causes an issue.
            /*
             * Relation data section. Likely a structure directly mapped to memory.
             * Isn't present if districtFlags equals 1, 4, or 5 (if the 0th and 2nd bits are true)
             * I've only seen layer_pc have a flag equal to 4 and rfgzone_pc either 1 or 0 (0 means they have relational_data)
             * Skip by default, since in my tests so far this entire section can be zeroed out without issue.
             * Can later add code to handle it if it turns out to be required.
             */
#if IgnoreRelationData
            if ((DistrictFlags & 5) == 0) //Relation data section only present when this condition is met.
            {
                reader.BaseStream.Seek(87368, SeekOrigin.Current); //Offset: 87392
            }
#else
            if ((DistrictFlags & 5) == 0)
            {
                //Totals to 87368
                var stupid_vtable_padding = reader.ReadBytes(4); //4
                var free = reader.ReadBytes(2); //2
                var slot = reader.ReadBytes(14560); //2 * 7280 //starts at 30
                var next = reader.ReadBytes(14560); //2 * 7280 //starts at 14590
                var more_stupid_padding = reader.ReadBytes(2); //2 //starts at 29150
                var entry_key = reader.ReadBytes(29120); //4 * 7280 //starts at 29152 //Might be 4 * numObjects bytes of data for keys, rest is useless, can be deleted/omitted. (Same with other arrays)
                var entry_data = reader.ReadBytes(29120); //4 * 7280 //starts at 58272
                //Ends at 87392
            }
#endif

            //Read zone objects
            for (int i = 0; i < NumObjects; i++)
            {
                var zoneObject = new ZoneObject();
                zoneObject.ReadFromBinary(stream);
                Objects.Add(zoneObject);
            }
#if DEBUG
            Console.WriteLine($"Last zone object ends at offset {reader.BaseStream.Position}");
#endif
        }

        /// <summary>
        /// Attempt to write the data stored in this class instance to outputPath. If you haven't already read data to
        /// this class instance then this won't work properly.
        /// </summary>
        /// <param name="outputPath">Path to write binary data to.</param>
        public void WriteToBinary(string outputPath)
        {
            WriteToBinary(new FileStream(outputPath, FileMode.Create));
        }

        /// <summary>
        /// Attempt to write the data stored in this class instance to stream. If you haven't already read data to
        /// this class instance then this won't work properly.
        /// </summary>
        /// <param name="stream">Stream to write binary data to.</param>
        public void WriteToBinary(Stream stream)
        {
            using var writer = new BinaryWriter(stream);
            //Write header
            writer.Write(0x454E4F5A); //Signature: Don't use value from input in case it's been changed to something incorrect
            writer.Write(36); //Version
            writer.Write(Objects.Count);
            writer.Write(NumHandles);
            writer.Write(DistrictHash);
            writer.Write(DistrictFlags);

            //Handle relation data section
            if ((DistrictFlags & 5) == 0) //Relation data section present if true. 
            {
                //Just fill it with zeros for now. Current knowledge points towards this section being useless, but not 100% certain yet
                writer.WriteNullBytes(87368);
            }
            
            //Write zone objects
            foreach (var zoneObject in Objects)
            {
                zoneObject.WriteToBinary(writer.BaseStream);
            }
        }

        /// <summary>
        /// Attempt to read xml data from inputPath.
        /// </summary>
        public void ReadFromXml(string inputPath)
        {
            var document = XDocument.Load(new FileStream(inputPath, FileMode.Open));
            var root = document.Root;
            if (root == null)
                throw new XmlException($"Input xml doc has no root node! Input path: \"{inputPath}\"");

            ReadFromXml(document);
        }

        /// <summary>
        /// Attempt to read xml data from document.
        /// </summary>
        public void ReadFromXml(XDocument document)
        {
            var root = document.Root;

            //Read header
            var header = root.GetRequiredElement("Header");

            Signature = header.GetRequiredAttributeValue("Signature").ToUint32();
            Version = header.GetRequiredAttributeValue("Version").ToUint32();
            NumObjects = header.GetRequiredAttributeValue("NumObjects").ToUint32();
            NumHandles = header.GetRequiredAttributeValue("NumHandles").ToUint32();
            DistrictHash = header.GetRequiredAttributeValue("DistrictHash").ToUint32();
            DistrictFlags = header.GetRequiredAttributeValue("DistrictFlags").ToUint32();

            //Would load relation data here if that section turns out to be necessary

            //Read zone objects
            var zoneObjectsNode = root.GetRequiredElement("ZoneObjects");
            foreach (var objectNode in zoneObjectsNode.Elements("ZoneObject"))
            {
                var zoneObject = new ZoneObject();
                zoneObject.ReadFromXml(objectNode);
                Objects.Add(zoneObject);
            }
            NumObjects = (uint)Objects.Count; //Update object count in case whoever edited the xml forgot to
        }

        /// <summary>
        /// Convert data in this class to xml and save it to outputPath.
        /// </summary>
        public void WriteToXmlAndSave(string outputPath)
        {
            var document = WriteToXml();
            document.Save(new FileStream(outputPath, FileMode.Create));
        }

        /// <summary>
        /// Write data stored in this class instance to an XDocument and return it.
        /// </summary>
        public XDocument WriteToXml()
        {
            //Xml file representation in memory
            var xml = new XDocument();

            //Add root node. Required for xml
            var root = new XElement("root");
            xml.Add(root);

            //Write header data
            var header = new XElement("Header");
            root.Add(header);
            header.Add(new XAttribute("Signature", Signature));
            header.Add(new XAttribute("Version", Version));
            header.Add(new XAttribute("NumObjects", NumObjects));
            header.Add(new XAttribute("NumHandles", NumHandles));
            header.Add(new XAttribute("DistrictHash", DistrictHash));
            header.Add(new XAttribute("DistrictFlags", DistrictFlags));

            var zoneObjects = new XElement("ZoneObjects");
            root.Add(zoneObjects);
            //Write zone objects (they handle writing their properties)
            foreach (var zoneObject in Objects)
            {
                zoneObjects.Add(zoneObject.WriteToXml());
            }

            //Return xml data for use
            return xml;
        }

        //Gets a unique ZoneObject num value for this zone file
        public uint GetUniqueNum()
        {
            for (uint i = 0; i < uint.MaxValue; i++)
            {
                bool foundMatch = false;
                foreach (var zoneObject in Objects)
                {
                    if (zoneObject.Num == i)
                    {
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch)
                    return i;
            }
            throw new Exception("Could not find a unique num value in zone file! You have way more objects than the game can manage!");
        }

        //Gets a unique ZoneObject handle value for this zone file
        public uint GetUniqueHandle()
        {
            for (uint i = 0; i < uint.MaxValue; i++)
            {
                bool foundMatch = false;
                foreach (var zoneObject in Objects)
                {
                    if (zoneObject.Handle == i)
                    {
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch)
                    return i;
            }
            throw new Exception("Could not find a unique handle value in zone file! You have way more objects than the game can manage!");
        }

        public ZoneObject DuplicateObject(int index)
        {
            return DuplicateObject(Objects[index]);
        }

        public ZoneObject DuplicateObject(ZoneObject obj)
        {
            int objIndex = Objects.IndexOf(obj);
            if (objIndex == -1)
                return null;

            var newObj = obj.Clone() as ZoneObject;
            if (newObj == null)
                return null;

            newObj.Num = GetUniqueNum(); //Further testing needed to see if this is necessary, likely can have same num as other objs as long as they are a different class
            newObj.Handle = GetUniqueHandle();

            Objects.Insert(objIndex + 1, newObj);
            return newObj;
        }
    }
}