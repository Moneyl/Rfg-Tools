using System;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Formats.Zones.Properties.Compound;
using RfgTools.Helpers;

namespace RfgTools.Formats.Zones.Properties.Special
{
    /// <summary>
    /// Navpoint data property. While it is a type 6 property, it doesn't inherit <see cref="ListProperty{T}"/>.
    /// This is since it's contents are partially guessed from disassembly of the game, so it's better to use named variables.
    /// </summary>
    [Property("navpoint_data", 6)]
    public class NavpointDataProperty : IProperty
    {
        //Property data, partially a guess
        public uint NavpointType;
        public uint UnkFlag1;
        public float Radius;
        public float SpeedLimit;
        public uint UnkFlag2;
        public uint UnkFlag3;
        public uint UnkVar1;

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

            if (size == 28)
            {
                var reader = new BinaryReader(stream);
                NavpointType = reader.ReadUInt32();
                UnkFlag1 = reader.ReadUInt32();
                Radius = reader.ReadSingle();
                SpeedLimit = reader.ReadSingle();
                UnkFlag2 = reader.ReadUInt32();
                UnkFlag3 = reader.ReadUInt32();
                UnkVar1 = reader.ReadUInt32();
                return true;
            }
            else
            {
                Console.WriteLine("Error! Found navpoint_data property with size != 28 bytes. Unknown data! Skipping property.");
                return false;
            }
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Type);
            writer.Write(Size);
            writer.Write(NameHash);

            writer.Write(NavpointType);
            writer.Write(UnkFlag1);
            writer.Write(Radius);
            writer.Write(SpeedLimit);
            writer.Write(UnkFlag2);
            writer.Write(UnkFlag3);
            writer.Write(UnkVar1);
        }

        public void ReadFromXml(XElement propertyRoot, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            var dataNode = propertyRoot.GetRequiredElement("Data");
            NavpointType = dataNode.GetRequiredValue("NavpointType").ToUint32();
            UnkFlag1 = dataNode.GetRequiredValue("UnkFlag1").ToUint32();
            Radius = dataNode.GetRequiredValue("Radius").ToSingle();
            SpeedLimit = dataNode.GetRequiredValue("SpeedLimit").ToSingle();
            UnkFlag2 = dataNode.GetRequiredValue("UnkFlag2").ToUint32();
            UnkFlag3 = dataNode.GetRequiredValue("UnkFlag3").ToUint32();
            UnkVar1 = dataNode.GetRequiredValue("UnkVar1").ToUint32();
        }

        public XElement WriteToXml()
        {
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, Name, TypeString);

            var dataNode = new XElement("Data");
            dataNode.Add(new XElement("NavpointType", NavpointType));
            dataNode.Add(new XElement("UnkFlag1", UnkFlag1));
            dataNode.Add(new XElement("Radius", Radius));
            dataNode.Add(new XElement("SpeedLimit", SpeedLimit));
            dataNode.Add(new XElement("UnkFlag2", UnkFlag2));
            dataNode.Add(new XElement("UnkFlag3", UnkFlag3));
            dataNode.Add(new XElement("UnkVar1", UnkVar1));
            propertyRoot.Add(dataNode);

            return propertyRoot;
        }
    }
}
