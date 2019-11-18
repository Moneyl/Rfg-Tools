using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Helpers;
using RfgTools.Types;

namespace RfgTools.Formats.Zones.Properties.Compound
{
    /// <summary>
    /// Used for list properties. Provides simple behavior for handling a list of values of one type.
    /// All list properties seen so far have had a type of 6.
    /// Doesn't have <see cref="PropertyAttribute"/> so <see cref="PropertyManager"/> ignores it.
    /// </summary>
    public class ListProperty<T> : IProperty
    {
        public List<T> Data { get; private set; } = new List<T>();
        public virtual string Name { get; protected set; } = "unknown";
        public virtual string TypeString { get; protected set; } = "list";

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

            int typeSize = SizeHelper.GetTypeSize<T>();
            
            var reader = new BinaryReader(stream);
            long startPos = reader.BaseStream.Position;

            if (!reader.TryReadList<T>(size, out var outList))
            {
                Console.WriteLine($"Error! Failed to read list data for \"{Name}\" property. Not divisible by {typeSize}. Skipping property.");
                return false;
            }
            Data = outList;
            return true;
        }

        public void WriteToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(Type);
            writer.Write(Size);
            writer.Write(NameHash);

            foreach (var item in Data)
            {
                writer.GenericWrite<T>(item);
            }
        }

        public void ReadFromXml(XElement propertyRoot, ushort type, ushort size, uint nameHash)
        {
            Type = type;
            Size = size;
            NameHash = nameHash;

            var dataNode = propertyRoot.GetRequiredElement("Data");
            foreach (var item in dataNode.Elements("Item"))
            {
                Data.Add(item.Cast<T>());
            }
            Size = (ushort)(SizeHelper.GetTypeSize<T>() * Data.Count);
        }

        public XElement WriteToXml()
        {
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, Name, TypeString);

            var dataNode = new XElement("Data");
            if (typeof(T).IsClass) //Todo: Handle this in a better way. Maybe by having T implement some IXmlSerializable. Unsure how that would work with primitive types
            {
                //For classes, need to have separate behavior. Attempt to find method called 'ToXElement' and call it
                //Admittedly a flimsy way of doing this, but it does the job for now.
                var conversionMethod = typeof(T).GetMethod("ToXElement");
                if (conversionMethod == null)
                {
                    throw new MissingMethodException(typeof(T).FullName, "ToXElement");
                }
                foreach (var item in Data)
                {
                    dataNode.Add((XElement)conversionMethod.Invoke(item, new object[]{"Item"}));
                }
            }
            else
            {
                //XElement can handle plain types
                foreach (var item in Data)
                {
                    dataNode.Add(new XElement("Item", item)); 
                }
            }

            propertyRoot.Add(dataNode);
            return propertyRoot;
        }
    }

    //All list properties below
    [Property("obj_links", 6)]
    class ObjLinksProperty : ListProperty<uint>
    {
        public override string Name { get; protected set; } = "obj_links";
    }

    [Property("world_anchors", 6)]
    class WorldAnchorsProperty : ListProperty<ushort>
    {
        public override string Name { get; protected set; } = "world_anchors";
    }

    [Property("covernode_data", 6)]
    class CovernodeDataProperty : ListProperty<uint>
    {
        public override string Name { get; protected set; } = "covernode_data";
    }

    [Property("dynamic_links", 6)]
    class DynamicLinksProperty : ListProperty<uint>
    {
        public override string Name { get; protected set; } = "dynamic_links";
    }

    [Property("path_road_struct", 6)]
    class PathRoadStructProperty : ListProperty<uint>
    {
        public override string Name { get; protected set; } = "path_road_struct";
    }

    [Property("path_road_data", 6)]
    class PathRoadDataProperty : ListProperty<ushort>
    {
        public override string Name { get; protected set; } = "path_road_data";
    }

    //Todo: List type is a guess. Each triangle takes up 6 bytes, guess is 2 byte value per point. That value likely being the index of point in yellow_polygon
    [Property("yellow_triangles", 6)]
    class YellowTrianglesProperty : ListProperty<ushort> 
    {
        public override string Name { get; protected set; } = "yellow_triangles";
    }

    //Todo: List type is a guess. Each triangle takes up 6 bytes, guess is 2 byte value per point. That value likely being the index of point in warning_polygon
    [Property("warning_triangles", 6)]
    class WarningTrianglesProperty : ListProperty<ushort> 
    {
        public override string Name { get; protected set; } = "warning_triangles";
    }

    [Property("yellow_polygon", 6)]
    class YellowPolygonProperty : ListProperty<vector2f>
    {
        public override string Name { get; protected set; } = "yellow_polygon";
    }

    [Property("warning_polygon", 6)]
    class WarningPolygonProperty : ListProperty<vector2f>
    {
        public override string Name { get; protected set; } = "warning_polygon";
    }
}
