using System;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Helpers;

namespace RfgTools.Formats.Zones.Properties.Special
{
    /// <summary>
    /// Preset property used when a property that has no definition or a definition mismatch.
    /// Doesn't have <see cref="PropertyAttribute"/> so <see cref="PropertyManager"/> ignores it.
    /// </summary>
    public class UnknownDataProperty : IProperty
    {
        public string DataAsBase64 { get; protected set; }
        public byte[] DataAsByteArray { get; protected set; }

        //Custom name, type, and data strings in case you wanted to do something like
        //"unknown_orient_type" in the case of a type mismatch.
        public readonly string CustomName;
        public readonly string CustomType;

        public ushort Type { get; protected set; }
        public ushort Size { get; protected set; }
        public uint NameHash { get; protected set; }

        public string GetFullName() => "Unknown data property";
        public string GetTypeName() => "Unknown property type";

        public UnknownDataProperty(string customName = "unknown", string customType = "unknown")
        {
            CustomName = customName;
            CustomType = customType;
        }

        public bool ReadFromStream(Stream stream, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            //Convert data to base64 string so it can be saved to xml.
            //This lets us reuse the data when that xml is converted back to a zone file.
            var reader = new BinaryReader(stream);
            DataAsByteArray = reader.ReadBytes(size);
            DataAsBase64 = Convert.ToBase64String(DataAsByteArray);
            return true;
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Type);
            writer.Write(Size);
            writer.Write(NameHash);

            writer.Write(DataAsByteArray);
        }

        public void ReadFromXml(XElement propertyRoot, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            DataAsBase64 = propertyRoot.GetRequiredValue("Data");
            DataAsByteArray = Convert.FromBase64String(DataAsBase64);
        }

        public XElement WriteToXml()
        {
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, CustomName, CustomType);


            propertyRoot.Add(new XElement("Data", DataAsBase64));
            return propertyRoot;
        }
    }
}
