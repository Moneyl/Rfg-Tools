using System;
using System.IO;
using System.Xml.Linq;
using RfgTools.Formats.Zones.Interfaces;
using RfgTools.Helpers;

namespace RfgTools.Formats.Zones.Properties.Primitive
{
    /// <summary>
    /// Used for uint properties. Provides simple behavior for handling a single uint since it's so common.
    /// All properties consisting of 1 uint so far have had the type of 5.
    /// Doesn't have <see cref="PropertyAttribute"/> so <see cref="PropertyManager"/> ignores it.
    /// </summary>
    public class UintProperty : IProperty
    {
        public uint Data { get; private set; } = 0;
        public virtual string Name { get; protected set; } = "unknown";
        public virtual string TypeString { get; protected set; } = "uint";

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

            if (size == 4)
            {
                var reader = new BinaryReader(stream);
                Data = reader.ReadUInt32();
                return true;
            }
            else
            {
                Console.WriteLine($"Error! Found uint property({Name}) with size != 4 byte. Unknown data! Skipping property.");
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

            Data = propertyRoot.GetRequiredValue("Data").ToUint32();
        }

        public XElement WriteToXml()
        {
            var propertyRoot = new XElement("Property");
            PropertyManager.WritePropertyInfoToXml(propertyRoot, Type, Size, NameHash, Name, TypeString);

            propertyRoot.Add(new XElement("Data", Data));
            return propertyRoot;
        }
    }


    //All uint properties below
    [Property("gm_flags", 5)]
    class GmFlagsProperty : UintProperty
    {
        public override string Name { get; protected set; } = "gm_flags";
    }

    [Property("dest_checksum", 5)]
    class DestChecksumProperty : UintProperty
    {
        public override string Name { get; protected set; } = "dest_checksum";
    }

    [Property("uid", 5)]
    class UidProperty : UintProperty
    {
        public override string Name { get; protected set; } = "uid";
    }

    [Property("next", 5)]
    class NextProperty : UintProperty
    {
        public override string Name { get; protected set; } = "next";
    }

    [Property("prev", 5)]
    class PrevProperty : UintProperty
    {
        public override string Name { get; protected set; } = "prev";
    }

    [Property("mtype", 5)]
    class MtypeProperty : UintProperty
    {
        public override string Name { get; protected set; } = "mtype";
    }

    [Property("group_id", 5)]
    class GroupIdProperty : UintProperty
    {
        public override string Name { get; protected set; } = "group_id";
    }

    [Property("ladder_rungs", 5)]
    class LadderRungsProperty : UintProperty
    {
        public override string Name { get; protected set; } = "ladder_rungs";
    }

    [Property("min_ambush_squads", 5)]
    class MinAmbushSquadsProperty : UintProperty
    {
        public override string Name { get; protected set; } = "min_ambush_squads";
    }

    [Property("max_ambush_squads", 5)]
    class MaxAmbushSquadsProperty : UintProperty
    {
        public override string Name { get; protected set; } = "max_ambush_squads";
    }

    [Property("host_index", 5)]
    class HostIndexProperty : UintProperty
    {
        public override string Name { get; protected set; } = "host_index";
    }

    [Property("child_index", 5)]
    class ChildIndexProperty : UintProperty
    {
        public override string Name { get; protected set; } = "child_index";
    }

    [Property("child_alt_hk_body_index", 5)]
    class ChildAltHkBodyIndexProperty : UintProperty
    {
        public override string Name { get; protected set; } = "child_alt_hk_body_index";
    }

    [Property("host_alt_hk_body_index", 5)]
    class HostAltHkBodyIndexProperty : UintProperty
    {
        public override string Name { get; protected set; } = "host_alt_hk_body_index";
    }

    [Property("host_handle", 5)]
    class HostHandleProperty : UintProperty
    {
        public override string Name { get; protected set; } = "host_handle";
    }

    [Property("child_handle", 5)]
    class ChildHandleProperty : UintProperty
    {
        public override string Name { get; protected set; } = "child_handle";
    }

    [Property("path_road_flags", 5)]
    class PathRoadFlagsProperty : UintProperty
    {
        public override string Name { get; protected set; } = "path_road_flags";
    }

    [Property("patrol_start", 5)]
    class PatrolStartProperty : UintProperty
    {
        public override string Name { get; protected set; } = "patrol_start";
    }

    [Property("yellow_num_points", 5)]
    class YellowNumPointsProperty : UintProperty
    {
        public override string Name { get; protected set; } = "yellow_num_points";
    }

    [Property("yellow_num_triangles", 5)]
    class YellowNumTrianglesProperty : UintProperty
    {
        public override string Name { get; protected set; } = "yellow_num_triangles";
    }

    [Property("warning_num_points", 5)]
    class WarningNumPointsProperty : UintProperty
    {
        public override string Name { get; protected set; } = "warning_num_points";
    }

    [Property("warning_num_triangles", 5)]
    class WarningNumTrianglesProperty : UintProperty
    {
        public override string Name { get; protected set; } = "warning_num_triangles";
    }

    [Property("pair_number", 5)]
    class PairNumberProperty : UintProperty
    {
        public override string Name { get; protected set; } = "pair_number";
    }

    [Property("group", 5)]
    class GroupProperty : UintProperty
    {
        public override string Name { get; protected set; } = "group";
    }

    [Property("priority", 5)]
    class PriorityProperty : UintProperty
    {
        public override string Name { get; protected set; } = "priority";
    }

    [Property("num_backpacks", 5)]
    class NumBackpacksProperty : UintProperty
    {
        public override string Name { get; protected set; } = "num_backpacks";
    }
}
