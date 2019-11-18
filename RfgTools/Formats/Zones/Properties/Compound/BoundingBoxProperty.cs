using System;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Zones.Properties.Compound
{
    [Property("bb", 5)]
    public class BoundingBoxProperty : IProperty
    {
        public vector3f Min { get; private set; }
        public vector3f Max { get; private set; }

        public ushort Type { get; protected set; }
        public ushort Size { get; protected set; }
        public uint NameHash { get; protected set; }

        public string GetFullName() => "Bounding box";
        public string GetTypeName() => "Bounding box";


        public bool ReadFromStream(Stream stream, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            if (size == 24)
            {
                var reader = new BinaryReader(stream);
                Min = reader.ReadVector3f();
                Max = reader.ReadVector3f();
                return true;
            }
            else
            {
                Console.WriteLine("Error! Found bounding box property with size != 24 bytes. Unknown data! Skipping property.");
                return false;
            }
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Type);
            writer.Write(Size);
            writer.Write(NameHash);

            writer.Write(Min);
            writer.Write(Max);
        }

        public void ReadFromXml(XElement propertyRoot, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            var dataNode = propertyRoot.GetRequiredElement("Data");
            Min = dataNode.GetRequiredElement("min").ToVector3f();
            Max = dataNode.GetRequiredElement("max").ToVector3f();
        }

        public XElement WriteToXml()
        {
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, "bb", "bb (bounding box)");

            var data = new XElement("Data");
            data.Add(Min.ToXElement("min"));
            data.Add(Max.ToXElement("max"));
            propertyRoot.Add(data);

            return propertyRoot;
        }
    }
}