using System;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Zones.Properties.Compound
{
    /// <summary>
    /// Used for vector3f properties. Provides simple behavior for handling a single vector3f since it's so common.
    /// All properties consisting of 1 vector3f so far have had the type of 5.
    /// Doesn't have <see cref="PropertyAttribute"/> so <see cref="PropertyManager"/> ignores it.
    /// </summary>
    public class Vector3fProperty : IProperty
    {
        public vector3f Data = new vector3f(0.0f, 0.0f, 0.0f);
        public virtual string Name { get; protected set; } = "unknown";
        public virtual string TypeString { get; protected set; } = "vector3f";

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

            if (size == 12)
            {
                var reader = new BinaryReader(stream);
                Data = reader.ReadVector3f();
                return true;
            }
            else
            {
                Console.WriteLine("Error! Found vector3f property with size != 12 bytes. Unknown data! Skipping property.");
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

            Data = propertyRoot.GetRequiredElement("Data").ToVector3f();
        }

        public XElement WriteToXml()
        {
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, Name, TypeString);

            propertyRoot.Add(Data.ToXElement("Data"));
            return propertyRoot;
        }
    }


    //All vector3f properties below
    [Property("just_pos", 5)]
    public class JustPosProperty : Vector3fProperty
    {
        public override string Name { get; protected set; } = "just_pos";
    }

    [Property("min_clip", 5)]
    class MinClipProperty : Vector3fProperty
    {
        public override string Name { get; protected set; } = "min_clip";
    }

    [Property("max_clip", 5)]
    class MaxClipProperty : Vector3fProperty
    {
        public override string Name { get; protected set; } = "max_clip";
    }

    [Property("clr_orig", 5)]
    class ClrOrigProperty : Vector3fProperty
    {
        public override string Name { get; protected set; } = "clr_orig";
    }
}
