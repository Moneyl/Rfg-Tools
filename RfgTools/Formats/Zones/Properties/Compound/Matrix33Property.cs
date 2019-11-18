using System;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Zones.Properties.Compound
{
    /// <summary>
    /// Used for matrix33 properties. 
    /// All properties consisting of 1 matrix33 so far have had the type of 5.
    /// Doesn't have <see cref="PropertyAttribute"/> so <see cref="PropertyManager"/> ignores it.
    /// </summary>
    public class Matrix33Property : IProperty
    {
        public matrix33 Data { get; private set; } = new matrix33(0.0f);
        public virtual string Name { get; protected set; } = "unknown";
        public virtual string TypeString { get; protected set; } = "matrix33";

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

            if (size == 36)
            {
                var reader = new BinaryReader(stream);
                Data = reader.ReadMatrix33();
                return true;
            }
            else
            {
                Console.WriteLine("Error! Found matrix33 property with size != 36 bytes. Unknown data! Skipping property.");
                return false;
            }
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Type);
            writer.Write(Size);
            writer.Write(NameHash);

            writer.Write(Data);
        }

        public void ReadFromXml(XElement propertyRoot, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            Data = propertyRoot.GetRequiredElement("Data").ToMatrix33();
        }

        public XElement WriteToXml()
        {
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, Name, TypeString);

            propertyRoot.Add(Data.ToXElement("Data"));
            return propertyRoot;
        }
    }


    //All matrix33 properties below
    [Property("nav_orient", 5)]
    class NavOrientProperty : Matrix33Property
    {
        public override string Name { get; protected set; } = "nav_orient";
    }
}
