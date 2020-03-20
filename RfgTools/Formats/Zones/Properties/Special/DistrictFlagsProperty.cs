using System;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Zones.Properties.Special
{
    //Todo: Double check that serialization works as expected for this with bit manipulation
    /// <summary>
    /// Bitfield used by district objects
    /// </summary>
    [Property("district_flags", 5)]
    public class DistrictFlagsProperty : IProperty
    {
        public DistrictFlags Data { get; private set; } = DistrictFlags.None;

        public virtual string Name { get; protected set; } = "navpoint_data";
        public virtual string TypeString { get; protected set; } = "navpoint_data";

        public ushort Type { get; protected set; }
        public ushort Size { get; protected set; }
        public uint NameHash { get; protected set; }

        public string GetFullName() => Name;
        public string GetTypeName() => TypeString;

        public bool ReadFromStream(Stream stream, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            if (size == 1)
            {
                var reader = new BinaryReader(stream);
                Data = (DistrictFlags)reader.ReadByte();
                return true;
            }
            else
            {
                Console.WriteLine("Error! Found district_flags property with size != 1 byte. Unknown data! Skipping property.");
                return false;
            }
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Type);
            writer.Write(Size);
            writer.Write(NameHash);

            writer.Write((int)Data);
        }

        public void ReadFromXml(XElement propertyRoot, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            var dataNode = propertyRoot.GetRequiredElement("Data");
            Data = (DistrictFlags)dataNode.GetRequiredValue("DistrictFlags").ToByte();
        }

        public XElement WriteToXml()
        {
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, Name, TypeString);

            var dataNode = new XElement("Data");
            dataNode.Add(new XElement("DistrictFlags", (byte)Data)); //Todo: Convert to individual bitflags for xml

            return propertyRoot;
        }
    }
}
