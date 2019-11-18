using System;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Zones.Properties.Compound
{
    /// <summary>
    /// Thought to mean "orient + position". Has a vector position, followed by a 3x3 orientation matrix.
    /// </summary>
    [Property("op", 5)]
    public class OpProperty : IProperty
    {
        public vector3f Position;
        public matrix33 Orient;

        public ushort Type { get; protected set; }
        public ushort Size { get; protected set; }
        public uint NameHash { get; protected set; }

        public string GetFullName() => "Op";
        public string GetTypeName() => "op";

        public bool ReadFromStream(Stream stream, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            if (size == 48)
            {
                var reader = new BinaryReader(stream);
                Position = reader.ReadVector3f();
                Orient = reader.ReadMatrix33();
                return true;
            }
            else
            {
                Console.WriteLine("Error! Found op (orient + position) property with size != 48 bytes. Unknown data! Skipping property.");
                return false;
            }
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Type);
            writer.Write(Size);
            writer.Write(NameHash);

            writer.Write(Position);
            writer.Write(Orient);
        }

        public void ReadFromXml(XElement propertyRoot, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            var dataNode = propertyRoot.GetRequiredElement("Data");
            Position = dataNode.GetRequiredElement("position").ToVector3f();
            Orient = dataNode.GetRequiredElement("orient").ToMatrix33();
        }

        public XElement WriteToXml()
        {
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, "op", "op (orient + position)");

            var data = new XElement("Data");
            data.Add(Position.ToXElement("position"));
            data.Add(Orient.ToXElement("orient"));
            propertyRoot.Add(data);

            return propertyRoot;
        }
    }
}
